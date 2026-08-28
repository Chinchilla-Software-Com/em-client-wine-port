using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace MailClient.Licensing.BouncyCastlePatch;

// Replaces MailClient.Licensing.DecryptAndVerify's RSA.Decrypt(data, RSAEncryptionPadding.OaepSHA1)
// calls. That native call goes through Windows CNG -> Wine's bcrypt.dll -> a GnuTLS backend that
// throws on RSA-OAEP private-key decrypt (gnutls_x509_privkey_set_spki hits an internal assertion
// -- confirmed via CX_DEBUGMSG trace, see reports/license-activation-oaep-findings.md).
// BouncyCastle's RSA/OAEP is pure managed code with no P/Invoke, so it never touches the broken
// code path.
//
// RSA-OAEP decrypt is a deterministic, standardized algorithm (RFC 8017) -- same key + same
// ciphertext + same OAEP parameters (hash, MGF, label) always produces the same plaintext,
// regardless of which correct implementation computes it. The key material is re-derived from
// the SAME already-loaded System.Security.Cryptography.RSA instance the original code uses
// (via ExportParameters, which does not go through the broken decrypt path), not hardcoded or
// duplicated anywhere -- this file never sees or stores actual key material at rest.
public static class OaepPatch
{
    // Mirrors RSAEncryptionPadding.OaepSHA1 exactly: SHA-1 digest, MGF1-SHA1, empty label.
    public static byte[] OaepSha1Decrypt(RSA rsa, byte[] data)
    {
        RSAParameters p = rsa.ExportParameters(includePrivateParameters: true);

        var privateKey = new RsaPrivateCrtKeyParameters(
            ToUnsignedBigInteger(p.Modulus!),
            ToUnsignedBigInteger(p.Exponent!),
            ToUnsignedBigInteger(p.D!),
            ToUnsignedBigInteger(p.P!),
            ToUnsignedBigInteger(p.Q!),
            ToUnsignedBigInteger(p.DP!),
            ToUnsignedBigInteger(p.DQ!),
            ToUnsignedBigInteger(p.InverseQ!));

        var oaep = new OaepEncoding(new RsaEngine(), new Sha1Digest(), new Sha1Digest(), null);
        oaep.Init(forEncryption: false, privateKey);
        return oaep.ProcessBlock(data, 0, data.Length);
    }

    private static BigInteger ToUnsignedBigInteger(byte[] bigEndianUnsigned) => new BigInteger(1, bigEndianUnsigned);
}
