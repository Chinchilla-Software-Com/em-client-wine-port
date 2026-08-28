# License Activation failure — RSA-OAEP decrypt broken in Wine's `bcrypt.dll`

> Found during testing around the same License dialog as the missing-icon bug
> (`supporting/register-icons-broken.png`) — unrelated root cause, a Windows CNG/crypto gap, not
> a rendering bug. Originally investigated as a separate, private track (kept out of `CLAUDE.md`
> at the time because the fix approach was still unproven); promoted to a public fix once the
> BouncyCastle replacement was built, verified cryptographically correct, and confirmed working
> end-to-end by the user.

## Symptom

Pressing **Activate** on the License dialog: a spinner appears briefly, then the dialog silently
reverts with no visible error and no crash — nothing an end user would recognize as a failure
message.

## Root cause

Traced with `CX_DEBUGMSG="+crypt,+bcrypt,+ncrypt,+secur32,+seh"`. Reproducing the Activate click
under trace, no bug-report file is written this time (unlike the two other public crashes fixed
earlier — this exception is caught internally, not surfaced), but the failure sequence is visible
directly in the trace:

```
BCryptOpenAlgorithmProvider(..., L"RSA", ...)
BCryptImportKeyPair(..., L"RSAPUBLICBLOB", ...)   -> succeeds
BCryptImportKeyPair(..., L"RSAPRIVATEBLOB", ...)  -> succeeds (with internal GnuTLS asserts, but returns a handle)
BCryptDecrypt(<private key handle>, ..., dwFlags=0x4)   <- 0x4 = BCRYPT_PAD_OAEP
  trace:crypt:gnutls_log <3> ASSERT: .../x509/privkey.c[gnutls_x509_privkey_set_spki]:1462
  GnuTLS error: The request is invalid.
  -> throws a managed exception (SEH code e0434352, info[0]=C10000E5) back through the interop call
```

**This is an RSA-OAEP private-key decrypt failing inside Wine's `bcrypt.dll`, which is
implemented on top of GnuTLS in this build.** `gnutls_x509_privkey_set_spki` — setting the
SubjectPublicKeyInfo/algorithm-parameters structure OAEP's more complex parameter encoding needs
(plain PKCS#1 v1.5 doesn't need this step, which is presumably why this bug is scoped to OAEP
specifically) — hits an internal GnuTLS assertion and fails. Wine's CNG shim doesn't translate
that failure into a clean NTSTATUS error return; it throws, which surfaces in .NET as a raw
exception through the P/Invoke boundary.

Exact call site, `MailClient.Licensing.DecryptAndVerify` (`MailClient.dll`):

```csharp
public class DecryptAndVerify
{
    private static RSA? signAlgorithm;
    private static RSA? encryptAlgorithm;

    public DecryptAndVerify(string signKeyXml, string encryptKeyXml)
    {
        signAlgorithm = RSA.Create();
        signAlgorithm.FromXmlString(signKeyXml);       // server's PUBLIC key -- verifies the
                                                         // server's signature on the response
        encryptAlgorithm = RSA.Create();
        encryptAlgorithm.FromXmlString(encryptKeyXml); // client's PRIVATE key -- decrypts the
                                                         // server's response, encrypted to it
    }

    public bool DecryptAndVerifyString(string inputString, out string? decryptedText)
    {
        try
        {
            // ... split into KeySize/8-byte chunks ...
            byte[] array3 = encryptAlgorithm.Decrypt(array2, RSAEncryptionPadding.OaepSHA1);
            // ... reassemble, then verify signature with SHA1/PKCS1 ...
        }
        catch (Exception e)
        {
            Program.ErrorReporterFactory.Create().ReportException(e, ReporterMode.Silent); // <-- SILENT
            decryptedText = null;
            return false;
        }
    }
    // DecryptAndVerifyStringV1 is the same shape, older wire format (XML body inside UTF32-decoded plaintext)
}
```

This explains the symptom end to end: `RSA.Decrypt(..., OaepSHA1)` hits the Wine/GnuTLS bug,
throws, gets caught by the blanket `catch (Exception e)`, reported via
`Program.ErrorReporterFactory...ReportException(e, ReporterMode.Silent)` — silent, unlike the
visible crash dialog seen for the two earlier public bugs (those use a different reporter mode) —
and `DecryptAndVerifyString` returns `false`. The Activate button's click handler, which depends
on a successful decrypt, just doesn't proceed: spinner, then back to the dialog, no visible error.

Constructed with `new DecryptAndVerify(BrandingUtils.ServerPublicKey, BrandingUtils.ClientPrivateKey)`
in every license-source class (`eMClientLicenseSource.cs`, `IcewarpLicenseSource.cs`,
`TurkCellLicenseSource.cs`, `MicrosoftStoreLicenseSource.cs`, `UpgradePreCheck.cs`) —
`eMClientLicenseSource.cs` (lines ~389, ~608) is the one behind the standard "Activate" button.
`BrandingUtils.ClientPrivateKey` is a fully local, embedded RSA private key (an XML string, per
`FromXmlString`) — no server-side secret, no hardware/certificate-store dependency. That's what
makes a local fix possible: the decrypt math needs no external material at all, just a correct
RSA-OAEP implementation that isn't Wine's broken CNG/GnuTLS one.

**What this is not:** not S/MIME or PGP email decryption. `BouncyCastle.Cryptography.dll` is
already vendored in this app for its actual OpenPGP work (`MailClient.Security.dll`'s
`PGPHelper`/`PGPEncryptor`) — that code is pure managed, never touches `bcrypt.dll`, and is
unaffected by this bug. Worth remembering if PGP/S-MIME is investigated separately later.

## Fix

Replaced the native `RSA.Decrypt(chunk, RSAEncryptionPadding.OaepSHA1)` call at both sites
(`DecryptAndVerifyString` and `DecryptAndVerifyStringV1`) with a call to a new pure-managed
helper, `MailClient.Licensing.BouncyCastlePatch.OaepPatch.OaepSha1Decrypt(RSA rsa, byte[] data)`,
built against the app's own already-vendored `BouncyCastle.Cryptography.dll`
(`Org.BouncyCastle.Crypto.Encodings.OaepEncoding` wrapping `Org.BouncyCastle.Crypto.Engines.RsaEngine`,
fed the same private-key material re-derived from the same `RSA` instance via `ExportParameters`
— no key material is duplicated or persisted anywhere new). Because BouncyCastle's RSA/OAEP is
pure managed code with no P/Invoke, it never touches Wine's broken `bcrypt.dll` code path at all.

RSA-OAEP decrypt is a deterministic, standardized algorithm (RFC 8017) — same key + same
ciphertext + same OAEP parameters (hash, MGF, label) always produces the same plaintext,
regardless of which correct implementation computes it — so this is a drop-in replacement, not a
behavioral change. Verified empirically before deploying: a standalone harness compared
`OaepPatch.OaepSha1Decrypt`'s output against native .NET `RSA.Decrypt` (which works correctly on
native Linux, only broken under Wine's CNG shim) across 4 RSA key sizes (1024/2048/3072/4096-bit)
× 4 plaintext scenarios (empty, short ASCII, max-OAEP-size, random bytes) — **32/32 checks
passed, byte-for-byte identical output in every case.**

Implementation: `il-patches/MailClient.Licensing.BouncyCastlePatch/` (the new helper assembly's
source — `OaepPatch.cs` + its `.csproj`) and `il-patches/license-oaep-patcher-Program.cs` (a
second, small Cecil-based patcher — kept separate from `~/tools/il-patcher` since this patch's
shape, redirecting a call to a newly-added sibling assembly, doesn't fit the existing tool's
per-fix flag pattern as cleanly as a new small tool). For each of the two target methods: deletes
the `call RSAEncryptionPadding::get_OaepSHA1()` instruction and retargets the following
`callvirt RSA::Decrypt(uint8[], RSAEncryptionPadding)` to `call OaepPatch::OaepSha1Decrypt(RSA,
uint8[])` (imported via `module.ImportReference` from the built helper assembly) — the stack
shape after removing the getter call, `[encryptAlgorithm, chunk]`, matches the new static
method's 2-parameter signature exactly. Neither instruction removed nor either retargeted
instruction is a branch target or exception-handler region boundary in either method, so none of
the three documented IL-patching pitfalls in `CLAUDE.md` apply to this specific patch.

Deploy also requires: copying `MailClient.Licensing.BouncyCastlePatch.dll` into the app folder
alongside `MailClient.dll`, and adding an entry for it to `MailClient.deps.json` (a real
deps.json-managed deployment — 247+ libraries listed — needs an explicit entry for a new assembly
to be resolved reliably at runtime; script: `il-patches/license-oaep-patcher-patch-deps-json.py`).
`BouncyCastle.Cryptography.dll` itself needs no new deployment step — it's already present and
already registered in `deps.json` as an existing app dependency.

## Verification method used

Followed the project's established preference for direct app-level instrumentation over trace
inference for questions about the app's own runtime state (see `CLAUDE.md`'s "Investigation
method", option 3): a temporary build logged entry/success/failure inside `OaepSha1Decrypt`
itself, plus a hook at the top of both methods' outer `catch (Exception ex)` blocks logging the
caught exception (if any) — inserted via `--patch-diag-catch`, an append-only insertion
immediately after the handler's own `stloc` that captures the exception, touching no branch
target or handler boundary. Both temporary hooks were removed from the tool and the helper
assembly once the fix was confirmed; they are not part of the deployed pipeline.

User reproduced by pressing Activate once. Log evidence (`Z:\tmp\claude-diag.log`):

```
[20:20:02.058] OaepPatch: OaepSha1Decrypt called, keySize=512, dataLen=64
[20:20:02.090] OaepPatch: OaepSha1Decrypt succeeded, outputLen=22
... (33 more chunks, all succeeded, zero FAILED lines)
[20:20:23.254] OaepPatch: OaepSha1Decrypt called, keySize=512, dataLen=64
... (34 more chunks, all succeeded, zero FAILED lines)
```

Two full decrypt sequences (the request/response round-trip), zero `FAILED` lines, and — equally
important — zero `DecryptAndVerify outer catch` lines, meaning the OAEP decrypt succeeded on
every chunk *and* the downstream SHA1/PKCS1 signature verification also succeeded (the method
returned `true` without ever entering its catch block).

## Confirmed working

User confirmed: License Activation completed successfully with no spinner-then-revert failure.

## Options considered and not pursued

- **Revive/finish a prior Azure-web-service redirect** (built months earlier, not completed): the
  user had previously extracted the encryption/decryption logic into a standalone C# web service
  and patched the assembly to call it instead of decrypting locally. Superseded by the
  BouncyCastle fix once proven — no external service, no network dependency, no added latency or
  failure mode, works offline. Not investigated further in this session since the local fix
  proved sufficient.
- **Ask the license server to use PKCS#1 v1.5 instead of OAEP:** not investigated — probably not
  controllable from the client side, and unnecessary once BouncyCastle proved to work.
