# MailClient — private investigation: encryption/decryption failure

Kept separate from `CLAUDE.md` (the public stabilization doc) at the user's request — this
covers a different track: a suspected Windows crypto API gap under Wine, and a private,
not-yet-working mitigation (an external Azure web service reimplementing the encryption/
decryption logic, called via a patched assembly instead of doing the crypto locally).

## Symptom — CONFIRMED (see "Root cause" below)

Pressing **Activate** on the License dialog: a spinner appears, then the dialog silently reverts
with no visible error. Root cause proven by trace: an RSA-OAEP decrypt inside Wine's `bcrypt.dll`
fails and throws; the app catches it and reports it in `ReporterMode.Silent`, which is why
nothing visible happens.

## Prior work (user's account, not yet verified against current state)

Several months ago (before this session), the user extracted the encryption/decryption logic
into a standalone C# web service, deployed it to Azure (works correctly there — real Windows
crypto available), and patched the assembly's internals to call that web service instead of
performing the crypto locally, sending the correct values back to the caller. Per the user:
this was not fully working / not completed. Not yet located in this session — needs finding
(check for any leftover patched DLLs, patch scripts, or service code/config) before assuming
its current state.

## Investigation channels confirmed valid for this Wine build

Via `strings /opt/cxoffice/lib/wine/i386-unix/<module>.so | grep -E '^[a-z0-9]+$'` (matches the
technique in `CLAUDE.md`'s "Investigation method" — channel names aren't guessable, confirm
first):
- `crypt` (crypt32.so — certificate store, CryptoAPI)
- `bcrypt` (bcrypt.so — CNG, modern symmetric/asymmetric primitives)
- `ncrypt` (PE-only, `i386-windows/ncrypt.dll` — CNG key storage; no separate unix .so, channel
  name confirmed via strings on the PE directly)
- `secur32` (secur32.so — SSPI, used for some certificate/auth flows)
- `rsaenh.dll` exists (PE-only) but has no matching self-named debug channel string found: legacy
  CryptoAPI CSP, likely traced under `crypt` if at all.

Routine `fixme:crypt:CRYPT_RegControl` / `fixme:crypt:CertAddCertificateLinkToStore` fixmes
appear on **every** startup regardless of whether any crypto feature is used (certificate store
init) — not meaningful on their own, don't mistake them for the target bug. Checked all trace
logs saved earlier in this session (public-investigation `.bak` files): same two fixmes in every
one, nothing else crypto-related — confirms a fresh, targeted trace with crypto channels enabled
is needed; the existing traces don't cover this.

## Root cause — CONFIRMED

Traced with `CX_DEBUGMSG="+crypt,+bcrypt,+ncrypt,+secur32,+seh"`. User reproduced: attempted an
encryption/decryption action, saw a spinner, then the app silently reverted to the previous
dialog (no crash, no bug-report file this time — the exception is caught somewhere internally,
unlike the two public crashes fixed earlier this session).

Found the exact failure in the trace (full sequence at the time — search the saved trace for
`get_gnutls_cipher handle block size` to relocate it if re-investigating):

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
SubjectPublicKeyInfo/algorithm-parameters structure needed specifically for OAEP's more complex
parameter encoding (vs. plain PKCS#1 v1.5, which doesn't need this step) — hits an internal
GnuTLS assertion and fails. Wine's CNG shim doesn't translate that failure into a clean NTSTATUS
error return; it throws, which surfaces in .NET as a raw exception through the P/Invoke boundary.

**What triggered it: pressing Activate on the License dialog** (`supporting/register-icons-broken.png`
from the public track — same dialog, unrelated icon bug), not S/MIME or PGP email decryption —
this is license activation. (BouncyCastle, which does this app's actual OpenPGP work
(`MailClient.Security.dll`'s `PGPHelper`/`PGPEncryptor`), is pure managed code with no native
P/Invoke and would never touch `bcrypt.dll` at all — worth remembering if email-side PGP/S-MIME
is investigated separately later, since it likely doesn't hit this same bug.) Exact call site
found, `MailClient.Licensing.DecryptAndVerify` (`MailClient.dll`):

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

This explains the exact observed symptom end to end: `RSA.Decrypt(..., OaepSHA1)` hits the
confirmed Wine/GnuTLS bug, throws, gets caught by the blanket `catch (Exception e)`, reported via
`Program.ErrorReporterFactory...ReportException(e, ReporterMode.Silent)` — **silent**, unlike the
visible crash dialog seen for the two public bugs earlier this session (which evidently use a
different reporter mode) — and `DecryptAndVerifyString` returns `false`. Whatever UI flow
depends on a successful decrypt (the Activate button's click handler) just doesn't proceed:
spinner, then back to the dialog, no visible error at all. Matches the reported symptom exactly.

**Constructed with `new DecryptAndVerify(BrandingUtils.ServerPublicKey, BrandingUtils.ClientPrivateKey)`**
in every license-source class found (`eMClientLicenseSource.cs`, `IcewarpLicenseSource.cs`,
`TurkCellLicenseSource.cs`, `MicrosoftStoreLicenseSource.cs`, `UpgradePreCheck.cs`) —
`eMClientLicenseSource.cs` (lines ~389, ~608) is almost certainly the one behind the standard
"Activate" button in the License dialog. `BrandingUtils.ClientPrivateKey` — not yet located
exactly, but by construction this must be a **fully local, embedded** RSA private key (an XML
string, per `FromXmlString`) — no server-side secret, no hardware/certificate-store dependency.
That's the important part for a fix: the actual decrypt math needs no external material at all,
just a correct RSA-OAEP implementation that isn't Wine's broken CNG/GnuTLS one.

## A cheaper fix than the Azure detour might exist

`BouncyCastle.Cryptography.dll` is already vendored in this app (used elsewhere for OpenPGP —
`MailClient.Security.dll`'s `PGPHelper`/`PGPEncryptor`). BouncyCastle's RSA/OAEP implementation
is **pure managed code** — no P/Invoke, no CNG, no dependency on Wine's `bcrypt.dll` at all. If
`DecryptAndVerify.encryptAlgorithm.Decrypt(chunk, RSAEncryptionPadding.OaepSHA1)` can be replaced
with the equivalent BouncyCastle call (`Org.BouncyCastle.Crypto.Encodings.OaepEncoding` wrapping
`Org.BouncyCastle.Crypto.Engines.RsaEngine`, fed the same private-key material re-parsed from the
same XML), the whole operation happens locally in managed code and never touches the broken Wine
code path — no external service, no added latency or failure mode, a genuinely local fix. This
would be a real IL patch (not trivial — needs the BouncyCastle key-parsing/OAEP call sequence
built via Cecil, and `MailClient.dll` may not currently reference `BouncyCastle.Cryptography.dll`
at all, which would need adding), but structurally more attractive than reviving the Azure
service: no infrastructure dependency, no network round-trip, works offline.

## Why this doesn't fit the "public fixes" pattern

Every bug fixed in the public track so far (`CLAUDE.md`) was either an application-level defect
(a mis-timed event, a missing style flag, a missing exception handler) fixable by patching
`MailClient`'s own IL, or a genuine but narrow Wine window-manager bug worked around from the
app side. This one is different: the RSA-OAEP failure is **inside Wine's own crypto backend**
(`bcrypt.dll` → GnuTLS), several layers below anything in `MailClient`'s control. There's no
"catch this differently-typed exception" trick available here the way there was for the
`IApplicationAssociationRegistration` bug — the underlying operation genuinely cannot succeed
through this code path in this Wine build. This is almost certainly *why* the user's prior
Azure-web-service approach exists: it's a legitimate, probably-necessary strategy for this
specific class of bug (bypass the broken local crypto primitive entirely, do the math somewhere
that has working RSA-OAEP, send back only the result), not a workaround reached for
prematurely.

## Status

Root cause fully proven and traced to its exact source (`DecryptAndVerify.DecryptAndVerifyString`/
`DecryptAndVerifyStringV1` in `MailClient.dll`, called from `eMClientLicenseSource.cs` behind the
Activate button). Not yet fixed. Options, roughly in order of how appealing they look right now:

1. **Replace the RSA-OAEP call with BouncyCastle's managed equivalent** (see above) — no
   external dependency, works offline, but real Cecil-authored IL work (new assembly reference
   possibly needed, BouncyCastle key-loading + OAEP-decrypt call sequence to build).
2. **Revive/finish the Azure web-service redirect** the user built previously — known to work in
   principle (Azure-side crypto is real Windows, not Wine), but wasn't fully working last time;
   needs finding/reviewing what exists first, and depends on network access + an external
   service staying up.
3. Not investigated: whether the *sending* side (the license server) could be asked to use
   PKCS#1 v1.5 instead of OAEP — probably not controllable from the client, likely a dead end,
   but worth a thought if 1 and 2 both stall.

Needs a decision from the user on which direction to pursue before building anything further.
