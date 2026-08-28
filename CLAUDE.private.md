# MailClient — private investigation: encryption/decryption failure

Kept separate from `CLAUDE.md` (the public stabilization doc) at the user's request — this
covers a different track: a suspected Windows crypto API gap under Wine, and a private,
not-yet-working mitigation (an external Azure web service reimplementing the encryption/
decryption logic, called via a patched assembly instead of doing the crypto locally).

## Symptom (as reported, not yet trace-confirmed)

Encryption/decryption operations (context not yet narrowed down — S/MIME? PGP? which UI action)
fail. Suspected cause: eM Client's crypto path depends on a Windows crypto API that Wine doesn't
implement (or doesn't fully implement) — not yet proven which one, or whether it's a Wine gap at
all vs. something Linux-side. Goal of this investigation round: prove what's actually happening
from a live trace before deciding on a fix approach.

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

## Status

Not yet started — first traced repro in progress.
