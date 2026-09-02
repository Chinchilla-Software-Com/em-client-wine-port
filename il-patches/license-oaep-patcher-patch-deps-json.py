#!/usr/bin/env python3
"""Add a deps.json entry for a sibling assembly (default: MailClient.Licensing.BouncyCastlePatch)
so the .NET runtime's deps.json-based assembly resolution finds it at runtime (MailClient.deps.json
lists 247 libraries -- this is a real deps.json-managed deployment, not one that falls back to bare
directory probing, so a new assembly dropped in the folder needs an explicit entry).

Generic since MailClient.Notifications.ButtonOverlay (the icon-overlay fix) needed the exact same
treatment as MailClient.Licensing.BouncyCastlePatch did -- genericized rather than duplicating this
file a second time. Name/version default to the original BouncyCastlePatch case so existing callers
(deploy.sh) don't need updating.

Usage: patch-deps-json.py <path-to-MailClient.deps.json> [assembly-name] [version]
Idempotent: safe to run again on an already-patched file (checks before adding).
"""
import json
import sys

NAME = sys.argv[2] if len(sys.argv) > 2 else "MailClient.Licensing.BouncyCastlePatch"
VERSION = sys.argv[3] if len(sys.argv) > 3 else "1.0.0"
KEY = f"{NAME}/{VERSION}"

path = sys.argv[1]
with open(path) as f:
    d = json.load(f)

tkey = ".NETCoreApp,Version=v8.0/win-x86"
targets = d["targets"][tkey]
root_key = next(k for k in targets if k.startswith("MailClient/"))

changed = False

if KEY not in targets[root_key]["dependencies"]:
    targets[root_key]["dependencies"][KEY.split("/")[0]] = VERSION
    changed = True

if KEY not in targets:
    targets[KEY] = {"runtime": {f"{NAME}.dll": {}}}
    changed = True

if KEY not in d["libraries"]:
    d["libraries"][KEY] = {"type": "project", "serviceable": False, "sha512": ""}
    changed = True

if changed:
    with open(path, "w") as f:
        json.dump(d, f, indent=2)
    print(f"OK: added {KEY} to {path}")
else:
    print(f"OK: {KEY} already present in {path}, no change")
