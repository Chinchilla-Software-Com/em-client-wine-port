#!/usr/bin/env python3
# Parses a .reg file laid out like office-associations.reg / common-attachments.reg (an
# extension-key stanza immediately followed by its ProgID's shell\open\command stanza, repeated,
# with blank lines and ";"-comment lines as the only things between/around pairs, each stanza
# containing exactly one `@="<value>"` default-value line -- confirmed the only shape these two
# generated files ever use) into one line per extension:
#   "<extension>\t<base64 of both paired stanzas, CRLF-joined>\t<JSON: [[key, data], [key, data]]>"
# Column 2 (the base64 blob) is for CrossOver: `wine regedit /S` a merged file built from these
# blobs. Column 3 (the JSON key/data pairs, HKEY_CLASSES_ROOT prefix stripped, backslash/quote
# escapes undone) is for Bottles: releases/<version>/deploy.sh calls `bottles-cli reg add -k
# "HKEY_CLASSES_ROOT\<key>" -v "" -d "<data>"` once per pair instead, since `reg.exe import` via
# `bottles-cli run` was confirmed NOT to reliably persist (see deploy.sh's own comment) while
# `bottles-cli reg add` (confirmed working, including for a HKEY_CLASSES_ROOT default value) is
# the reliable primitive there. Not meant to be a general .reg parser: relies on this project's
# own generated file shape (one `@="..."` line per stanza, nothing else).
import base64
import json
import re
import sys

def unescape_reg_string(s):
    # Undoes .reg's own quoting: \\ -> \, \" -> "
    out = []
    i = 0
    while i < len(s):
        if s[i] == "\\" and i + 1 < len(s):
            out.append(s[i + 1])
            i += 2
        else:
            out.append(s[i])
            i += 1
    return "".join(out)

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
    key_re = re.compile(r"^\[HKEY_CLASSES_ROOT\\(.+)\]$")
    default_value_re = re.compile(r'^@="(.*)"$')

    def stanza_key_and_data(stanza):
        m = key_re.match(stanza[0])
        key = m.group(1) if m else None
        data = None
        for line in stanza[1:]:
            dm = default_value_re.match(line)
            if dm:
                data = unescape_reg_string(dm.group(1))
                break
        return key, data

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

        pairs = []
        for stanza in (stanzas[j], stanzas[j + 1]):
            key, data = stanza_key_and_data(stanza)
            if key is not None and data is not None:
                pairs.append([key, data])
        pairs_json = json.dumps(pairs)

        print(f"{ext}\t{encoded}\t{pairs_json}")
        j += 2

if __name__ == "__main__":
    main()
