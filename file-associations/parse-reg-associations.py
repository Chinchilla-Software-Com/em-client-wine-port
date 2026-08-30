#!/usr/bin/env python3
# Parses a .reg file laid out like office-associations.reg / common-attachments.reg (an
# extension-key stanza immediately followed by its ProgID's shell\open\command stanza, repeated,
# with blank lines and ";"-comment lines as the only things between/around pairs) into one line
# per extension: "<extension>\t<base64 of both paired stanzas, CRLF-joined, ready to paste into
# another .reg file>". Used by releases/<version>/deploy.sh to check each extension against the
# target bottle's live registry individually and import only the ones actually missing (or all of
# them, with --force-associations) -- see deploy.sh's own comments for the merge logic. Not meant
# to be a general .reg parser: relies on this project's own generated file shape.
import base64
import re
import sys

def main():
    if len(sys.argv) != 2:
        print("usage: parse-reg-associations.py <path-to.reg>", file=sys.stderr)
        sys.exit(1)

    with open(sys.argv[1], encoding="utf-8-sig") as f:
        text = f.read()

    lines = text.replace("\r\n", "\n").split("\n")
    stanzas = []  # list of list[str], each a bracketed section's lines (header + body)
    i, n = 0, len(lines)
    while i < n:
        line = lines[i]
        if line.startswith("[") and line.endswith("]"):
            body = [line]
            i += 1
            while i < n and lines[i].strip() != "" and not lines[i].startswith(";"):
                body.append(lines[i])
                i += 1
            stanzas.append(body)
        else:
            i += 1

    ext_re = re.compile(r"^\[HKEY_CLASSES_ROOT\\\.(\w+)\]$")
    j = 0
    while j + 1 < len(stanzas):
        header_a = stanzas[j][0]
        m = ext_re.match(header_a)
        if not m:
            j += 1
            continue
        ext = m.group(1)
        block_text = "\r\n".join(stanzas[j]) + "\r\n\r\n" + "\r\n".join(stanzas[j + 1]) + "\r\n"
        encoded = base64.b64encode(block_text.encode("utf-8")).decode("ascii")
        print(f"{ext}\t{encoded}")
        j += 2

if __name__ == "__main__":
    main()
