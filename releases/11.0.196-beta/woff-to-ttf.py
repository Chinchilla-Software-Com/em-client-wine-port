#!/usr/bin/env python3
"""Minimal WOFF (v1) -> TTF/SFNT converter, stdlib only (zlib). Not WOFF2 (brotli+transforms,
much more complex) -- WOFF1's tables are just individually zlib-deflated, trivial to reconstruct.

Why this exists: used by install-msix.sh to install Roboto (Apache-2.0, genuinely freely
redistributable, unlike the Microsoft fonts elsewhere in this pipeline). No source was found
serving real standalone .ttf files directly for it -- google/fonts' own GitHub repo has moved
Roboto out to its own source-only repo (no built binaries), and fonts.google.com's own /download
endpoint returns an interactive HTML page, not a file, for a plain scripted GET. The most
reliable available fetch is Google's own CDN (fonts.gstatic.com, via the widely-used
`typeface-roboto` npm package's file listing on the jsdelivr CDN mirror), which only serves
`.woff`/`.woff2` -- hence converting here rather than fetching a `.ttf` directly. WOFF1's tables
are stored uncompressed-or-individually-deflated with an otherwise-unmodified SFNT table
directory, so this reconstruction is lossless: same glyph outlines, same metrics, same name
table, just re-wrapped from WOFF's container back into a plain SFNT/TTF one. Verified against a
real render (PIL/FreeType) before use -- see reports/emclient11-startup-stack-overflow-findings.md.
"""
import struct, sys, zlib

def sfnt_checksum(data: bytes) -> int:
    padded = data + b"\x00" * ((4 - len(data) % 4) % 4)
    total = 0
    for i in range(0, len(padded), 4):
        total = (total + struct.unpack(">I", padded[i:i+4])[0]) & 0xFFFFFFFF
    return total

def woff_to_ttf(src: str, dst: str):
    data = open(src, "rb").read()
    sig, flavor, length, num_tables, reserved, total_sfnt_size, major, minor, \
        meta_off, meta_len, meta_orig_len, priv_off, priv_len = struct.unpack(">4sIIHHIHHIIIII", data[:44])
    assert sig == b"wOFF", f"not a WOFF file: {sig!r}"

    tables = []
    off = 44
    for _ in range(num_tables):
        tag, toffset, complen, origlen, origchecksum = struct.unpack(">4sIIII", data[off:off+20])
        off += 20
        raw = data[toffset:toffset+complen]
        table_data = zlib.decompress(raw) if complen != origlen else raw
        assert len(table_data) == origlen, f"length mismatch for {tag}"
        tables.append((tag, table_data))

    tables.sort(key=lambda t: t[0])

    num = len(tables)
    entry_selector = num.bit_length() - 1 if num > 0 else 0
    search_range = (1 << entry_selector) * 16
    range_shift = num * 16 - search_range

    out = bytearray()
    out += struct.pack(">IHHHH", flavor, num, search_range, entry_selector, range_shift)
    dir_offset = len(out)
    out += b"\x00" * (16 * num)

    data_start = len(out)
    entries = []
    head_checksum_pos = None
    cursor = data_start
    for tag, tdata in tables:
        padded = tdata + b"\x00" * ((4 - len(tdata) % 4) % 4)
        checksum = sfnt_checksum(tdata)
        entries.append((tag, checksum, cursor, len(tdata)))
        if tag == b"head":
            head_checksum_pos = cursor + 8  # checkSumAdjustment field offset within 'head'
        out += padded
        cursor += len(padded)

    # Zero the head table's checkSumAdjustment before computing the whole-file checksum.
    if head_checksum_pos is not None:
        out[head_checksum_pos:head_checksum_pos+4] = b"\x00\x00\x00\x00"

    for i, (tag, checksum, tabs_off, tab_len) in enumerate(entries):
        struct.pack_into(">4sIII", out, dir_offset + i * 16, tag, checksum, tabs_off, tab_len)

    file_checksum = sfnt_checksum(bytes(out))
    checksum_adjustment = (0xB1B0AFBA - file_checksum) & 0xFFFFFFFF
    if head_checksum_pos is not None:
        struct.pack_into(">I", out, head_checksum_pos, checksum_adjustment)

    open(dst, "wb").write(out)

if __name__ == "__main__":
    woff_to_ttf(sys.argv[1], sys.argv[2])
