#!/usr/bin/env python3
"""Add a deps.json entry for a sibling assembly (default: MailClient.Licensing.BouncyCastlePatch)
so the .NET runtime's deps.json-based assembly resolution finds it at runtime (MailClient.deps.json
lists 247 libraries -- this is a real deps.json-managed deployment, not one that falls back to bare
directory probing, so a new assembly dropped in the folder needs an explicit entry).

Generic since MailClient.Notifications.ButtonOverlay (the icon-overlay fix) needed the exact same
treatment as MailClient.Licensing.BouncyCastlePatch did -- genericized rather than duplicating this
file a second time. Name/version default to the original BouncyCastlePatch case so existing callers
(deploy.sh) don't need updating.

Usage: patch-deps-json.py <path-to-MailClient.deps.json> [assembly-name] [version] [target-key]
Idempotent: safe to run again on an already-patched file (checks before adding).

target-key defaults to the 10.4.5674 (net8.0) pipeline's own target framework moniker/RID pair --
the eM Client 11.0.196-beta pipeline (net10.0) needs ".NETCoreApp,Version=v10.0/win-x86" passed
explicitly (confirmed against a live MailClient.deps.json: it lists both
".NETCoreApp,Version=v10.0" and ".NETCoreApp,Version=v10.0/win-x86" as top-level target keys --
the RID-qualified one is the one carrying the actual per-library "dependencies"/"runtime" entries
this script edits, same shape as v10's own win-x86-qualified key).
"""
import json
import sys

NAME = sys.argv[2] if len(sys.argv) > 2 else "MailClient.Licensing.BouncyCastlePatch"
VERSION = sys.argv[3] if len(sys.argv) > 3 else "1.0.0"
TARGET_KEY = sys.argv[4] if len(sys.argv) > 4 else ".NETCoreApp,Version=v8.0/win-x86"
KEY = f"{NAME}/{VERSION}"

path = sys.argv[1]
with open(path) as f:
    d = json.load(f)

targets = d["targets"][TARGET_KEY]
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
