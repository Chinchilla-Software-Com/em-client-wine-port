# Crash when a message from a specific sender is shown (GitHub issue #4)

## Symptom
eM Client 10 shows its error dialog when a mail from one particular sender is displayed in the preview
pane (also on arrival, and for the copy sitting in Trash). Message box text:
`Cannot send error report, error ".', hexadecimal value 0x00, is an invalid character. Line 1195, position 26.`
That text is the known red herring (the crash-report XML contains a NUL char, so *sending* fails) -- the
real exception is in the `bug.*.txt` file.

## Root cause
Identical stack in `bug.20260928105032.txt`, `bug.20261007194251.txt`-era and `bug.20261102004747.txt`:

    ExternalException "A generic error occurred in GDI+."
      Image.FromStream <- AvatarHelper.FromData <- UIAvatar.get_Image/GetImage
      <- MessageBehaviorHandlerBase.StoreToCache/GetContactUrl/GetHtmlForMessageHeader
      <- ControlMessageDetail.InsertMessagesInternal <- webBrowser_LoadEnd (unhandled)

The *sender's contact photo* (not the mail body/footer images) can't be decoded by Wine's GDI+.
`AvatarHelper.FromData` already handles that: `catch (ExternalException ex) { if (ex.Source ==
"System.Drawing") return null; throw; }`. On current .NET the exception originates in
`System.Private.Windows.Core`, so the check is false and it rethrows. (The exact bytes that GDI+
rejects were not inspected.)

## Fix (Stage 17 / patch 7, mandatory)
`--patch-avatar-gdiplus-source-check`: replace the `brfalse.s -> rethrow` after the `Source ==` compare
with `pop` (same stack effect), so the handler always returns null and the caller falls back to the
no-photo avatar. One instruction, MailClient.dll only. Verified: decompile shows
`catch (ExternalException ex) { _ = ex.Source == "System.Drawing"; return null; }`, `--dump-handlers`
OK, second run refuses (shape no longer present).

## eM Client 11
`AvatarHelper.FromData` is byte-for-byte the same shape in the 11.0.196 build (checked via `--dump-il`); wired into
`releases/11.0.196-beta/deploy.sh` as Stage 9 / patch 7. Not yet run against a live eM Client 11 bottle.

## Not yet done
- Visual confirmation with the Trash mail (10.4.5674 deployed to the Lutris prefix `em-client-10`, backup in
  `~/Lutris/em-client-10-backup-avatar/`).
- Not tested: a full `deploy.sh` run with Stage 17 (only `bash -n`).
