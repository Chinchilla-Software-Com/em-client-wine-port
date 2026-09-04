# eM Client 11 (beta) startup crash: PBKDF2 key derivation broken under Wine's bcrypt

**Status: fixed and deployed as release/11.0.196-1.** Confirmed working live — the app now
reaches a normal Inbox window instead of crashing before the splash screen finishes.

This is a **separate product line** from the rest of this repo's fixes: eM Client 11.0.196-beta
is a different, still-changing build (different assembly set, new .NET 10 target) tested in its
own bottle (`emClient_11_beta_win_11`) with its own release pipeline
(`releases/11.0.196-beta/deploy.sh`), independent of the mature `releases/10.4.5674/` pipeline.
See CLAUDE.md's "eM Client 11 (beta) — separate release line" section for how the two are kept
apart.

## Symptom

eM Client 11.0.196-beta would not start at all in a fresh `emClient_11_beta_win_11` CrossOver
bottle — it crashed during its own background initialization, before the main window (or even a
fully-drawn splash screen) appeared.

## Root cause

The app's own crash report (`bug.<timestamp>.txt` in the bottle's
`drive_c/users/crossover/AppData/Local/Temp/`) pointed straight at it:

```
System.Security.Cryptography.CryptographicException: Unknown error (0xc1000008)
  at System.Security.Cryptography.Pbkdf2Implementation.FillKeyDerivation
  at System.Security.Cryptography.Pbkdf2Implementation.Fill
  at System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2Core / Pbkdf2 (x2 overloads)
  at MailClient.Utils.Security.Cryptography.AESEncryptor.EncryptStringAES
  at MailClient.Security.MasterPasswordManager.Encrypt
  at MailClient.UI.FileBasedCache`1.Initialize
  at MailClient.Program.InitOnBackground
  at MailClient.Program+<>c.<RunInitOnBackground>b__231_0
```

eM Client 11 targets .NET 10. .NET 10's `Rfc2898DeriveBytes.Pbkdf2(...)` **static** method
(the modern, recommended PBKDF2 API) always routes through the OS's native CNG crypto layer on
Windows — `BCryptDeriveKeyPBKDF2`/`BCryptKeyDerivation`. Wine's `bcrypt.dll` DOES export both of
those functions (confirmed via `objdump -p` against
`/opt/cxoffice/lib/wine/i386-windows/bcrypt.dll` — ordinals 9 and 37 respectively), but calling
through to them throws immediately. This is a distinct gap from the RSA-OAEP `bcrypt` issue
already fixed for the stable 10.4.5674 release (see
`reports/license-activation-oaep-findings.md`) — same general "Wine's bcrypt/GnuTLS backend is
missing something .NET now depends on" shape, but a completely different CNG entry point
(PBKDF2 key derivation, not RSA-OAEP decrypt), and this one blocks **startup itself**, not just
license activation.

### Confirmed universal, not app-specific

Built a minimal standalone `net10.0`/`win-x86` console app and ran it directly inside the same
bottle via `cxstart`:

```csharp
Rfc2898DeriveBytes.Pbkdf2("password", Encoding.UTF8.GetBytes("salt1234"), 1000, HashAlgorithmName.SHA1, 32);
Rfc2898DeriveBytes.Pbkdf2("password", Encoding.UTF8.GetBytes("salt1234"), 1000, HashAlgorithmName.SHA256, 32);
```

Both calls failed with the exact same `CryptographicException: Unknown error (0xc1000008)` —
confirming this is a genuine, universal Wine gap, not something particular to eM Client's own
password/salt/iteration values.

### The old instance-based API works fine

The same repro tool also confirmed a workaround exists **within the same BCL assembly**: the
older, `[Obsolete]`-marked instance-based API —

```csharp
using var kdf = new Rfc2898DeriveBytes(password, salt, iterations, hashAlgorithmName);
byte[] key = kdf.GetBytes(outputLength);
```

— succeeds under the exact same Wine build. So does raw `HMACSHA1`/`HMACSHA256` (confirming
hashing/HMAC itself isn't broken — this is specifically the new static method's CNG-only
dispatch path that's missing something). This made the fix much simpler than the earlier
RSA-OAEP gap: no new BouncyCastle-backed helper assembly needed (see
`reports/license-activation-oaep-findings.md` for that heavier fix) — both the broken static
method and the working instance API live in the same already-referenced
`System.Security.Cryptography` assembly, so the fix is a pure IL-level call-site swap.

## Fix

`il-patches/il-patcher-Program.cs` gained `--patch-pbkdf2-instance-api`. Rather than hardcoding
one type/method (the first crash pointed only at `AESEncryptor`), it **scans every
`MailClient*.dll`** for any call matching the exact broken signature —
`Rfc2898DeriveBytes.Pbkdf2(string, byte[], int, HashAlgorithmName, int)` — and replaces each one
in place:

```
call Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithmName, outputLength)
```
becomes
```
stloc  outputLengthLocal          ; stash the last-pushed arg first (newobj only wants 4)
newobj Rfc2898DeriveBytes::.ctor(password, salt, iterations, hashAlgorithmName)
ldloc  outputLengthLocal
callvirt Rfc2898DeriveBytes::GetBytes(outputLengthLocal)
```

The new `MethodReference`s (ctor, `GetBytes`) are built by hand off the **existing** Pbkdf2 call
instruction's own `DeclaringType`/parameter-type references, not `typeof()` reflection on the
patcher tool's own runtime — sidesteps the app-local-assembly-version risk documented as
IL-patching lesson 5 in CLAUDE.md.

### Two independent call sites found, not one

Deploying a first version of the patch that only touched `AESEncryptor.EncryptStringAES`/
`DecryptStringAES` got the app *past* that crash — but immediately hit a **second**, independent
`CryptographicException` at the exact same message, this time from
`MailClient.UI.FileBasedCache<TEntry>.Initialize` (in `MailClient.Abstractions.dll`, a different
assembly) calling the same broken static overload directly, inside its own try/filter-catch
region. This is why `--patch-pbkdf2-instance-api` scans generically for the call shape across
every app assembly rather than hardcoding call sites one at a time — whack-a-moling each crash
individually would very likely have missed sites that only trigger later in the app's
initialization sequence (or with different account/cache configurations). One patch run found
and fixed all 3 sites that exist in this build (2 in `MailClient.dll`, 1 in
`MailClient.Abstractions.dll`); a future 11.x build could have more or fewer, and the same patch
would still find whatever's actually there (it fails loudly if it finds zero, as a sanity check).

### A non-.NET DLL almost broke the scan

`MailClient.Mapi.dll` is a native MAPI COM interop shim — a plain Win32 PE DLL with no CLR
header, despite matching the `MailClient*.dll` glob. The first version of the generic scan
called `ModuleDefinition.ReadModule` unconditionally on every matched filename and crashed the
whole batch with `BadImageFormatException` the moment it reached that file. Fixed by wrapping
the read in a `try/catch (BadImageFormatException)` and copying the file through unpatched —
worth remembering for any future "scan every `MailClient*.dll`" style patch in this project,
since native interop shims are apparently part of this app's naming convention too.

### Verification

- Decompiled both patched methods (`AESEncryptor.EncryptStringAES`/`DecryptStringAES`,
  `FileBasedCache<T>.Initialize`) and confirmed the new
  `new Rfc2898DeriveBytes(...).GetBytes(...)` shape, with the old broken static call gone.
- `--dump-handlers` on `FileBasedCache<T>.Initialize` (whose Pbkdf2 call sits inside an existing
  try/filter-catch region, not at its boundary) confirmed the handler table's nesting is
  unchanged before and after patching — 3 regions, same nesting order, just later byte offsets
  from the inserted instructions.
- Live end-to-end: `releases/11.0.196-beta/deploy.sh` run against a bottle restored to pristine
  (`original/em-11.0.196/`), full pipeline (build, patch, verify, backup, deploy) succeeded, and
  the app launched to a real Inbox window with no crash report. Re-ran the script a second time
  against the now-patched bottle to confirm the revision marker correctly short-circuits (skips
  the whole pipeline, "already at release 11.0.196-1").

## Tooling notes specific to this investigation

- **Standalone repro executables are worth building** when a crash report alone doesn't say
  whether something is universally broken or specific to the app's own inputs — a `net10.0`/
  `win-x86` framework-dependent console app, published and run via `cxstart` directly inside the
  target bottle, settled both the "is this universal" and "does the old API work" questions in
  minutes, far faster than instrumenting the real app and re-deploying to test a hypothesis.
- **`objdump -p <wine-dll> | grep -i <api-name>`** confirms whether a Windows API is exported at
  all by a given Wine build before assuming it's simply missing — `BCryptDeriveKeyPBKDF2` and
  `BCryptKeyDerivation` are both genuinely exported here; the bug is in what they *do*, not
  whether they exist, which ruled out "just call a different, unimplemented-here API" as a
  possible angle before it wasted any time.
