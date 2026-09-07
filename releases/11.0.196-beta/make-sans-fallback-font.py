#!/usr/bin/env python3
"""Generates a minimal font whose Windows-platform 'name' table family/full-name records are the
literal 4-character string "sans", by patching a copy of an already-installed real font.

Why this exists: see reports/emclient11-startup-stack-overflow-findings.md ("Bug B") in this
repo for the full investigation. Short version -- Chromium's font code
(ui/gfx/platform_font_skia.cc) falls back to a hardcoded last-resort family name, literally
"sans", when a requested font can't be resolved. Real Windows always has *something* answer to
that name via OS-level generic-family resolution; a fresh Wine/CrossOver bottle does not, by
default. When DirectWrite's FindFamilyName("sans") comes back "not found", a guard in Chromium's
own font-initialization code that's supposed to make this a one-time, cleanly-handled failure
does not hold under Wine, and the app recurses until it stack-overflows its own UI thread during
startup -- the underlying cause of a "white screen" freeze that looks like (but is NOT) the
Wine paint/layered-window bugs already fixed elsewhere in this project.

This is deliberately a *generator*, not a vendored binary font file: the output is a modified
derivative of whatever real font it's based on (Arial, by default -- see the caller), and this
repo's fonts/*.ttf convention is for genuine, unmodified, license-held font files only. Regenerate
fresh from whatever's actually installed in the target bottle at install time instead, the same
"never vendor a pre-built derivative" philosophy this repo already applies to DLL patches.

Only touches Windows-platform (platformID=3) name records for family (nameID 1) and full name
(nameID 4) -- the only ones DirectWrite consults for FindFamilyName. Always SHRINKS the string
(the source font's own name, e.g. "Arial", is always longer than "sans"), which is safe without
needing to restructure the name table's string-offset layout: each record's own length field is
updated to the new (shorter) length, and the now-unused trailing bytes of the old string are left
in place but ignored by any correct parser. A Macintosh-platform (platformID=1) record that
happens to share the same underlying string storage as an optimization (common in real fonts) can
end up with harmless garbled bytes as a side effect -- irrelevant here, since DirectWrite (and
this fix's entire purpose) only cares about Windows-platform records.

No external dependencies (no fontTools) -- this project's target environments can't reliably
install Python packages, so this is a minimal, self-contained binary patcher using only the
stdlib.
"""

import struct
import sys
import shutil


def make_sans_fallback_font(src_path: str, dst_path: str) -> None:
    shutil.copyfile(src_path, dst_path)

    with open(dst_path, "r+b") as f:
        data = bytearray(f.read())

        num_tables = struct.unpack_from(">H", data, 4)[0]
        name_off = None
        for i in range(num_tables):
            rec_off = 12 + i * 16
            tag = bytes(data[rec_off:rec_off + 4])
            if tag == b"name":
                name_off = struct.unpack_from(">I", data, rec_off + 8)[0]
                break
        if name_off is None:
            raise ValueError(f"{src_path}: no 'name' table found")

        _fmt, count, string_off = struct.unpack_from(">HHH", data, name_off)
        string_off_abs = name_off + string_off

        new_str = "sans".encode("utf-16-be")
        patched = []
        for i in range(count):
            rec_off = name_off + 6 + i * 12
            platform_id, enc_id, lang_id, name_id, length, offset = struct.unpack_from(
                ">HHHHHH", data, rec_off
            )
            # Windows platform (3), family (1) or full name (4) -- the only records DirectWrite's
            # FindFamilyName consults.
            if platform_id == 3 and name_id in (1, 4):
                if length < len(new_str):
                    raise ValueError(
                        f"{src_path}: name record (platform={platform_id}, nameID={name_id}) is "
                        f"only {length} bytes, too short to shrink to \"sans\" ({len(new_str)} "
                        f"bytes) safely -- pick a different source font"
                    )
                str_off_abs = string_off_abs + offset
                data[str_off_abs:str_off_abs + len(new_str)] = new_str
                struct.pack_into(">H", data, rec_off + 8, len(new_str))
                patched.append((platform_id, enc_id, lang_id, name_id))

        if not patched:
            raise ValueError(f"{src_path}: no Windows-platform family/full-name records found to patch")

        f.seek(0)
        f.write(data)
        f.truncate()

    print(f"patched {len(patched)} name record(s) in {dst_path}: {patched}", file=sys.stderr)


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print(f"usage: {sys.argv[0]} <source-font.ttf> <output-font.ttf>", file=sys.stderr)
        sys.exit(1)
    make_sans_fallback_font(sys.argv[1], sys.argv[2])
