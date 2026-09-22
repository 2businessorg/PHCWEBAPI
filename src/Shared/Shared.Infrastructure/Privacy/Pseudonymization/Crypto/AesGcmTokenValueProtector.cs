using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Options;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Crypto;

/// <summary>
/// AES-GCM protector for TokenMap values. Key from options or ephemeral process key.
/// </summary>
public sealed class AesGcmTokenValueProtector : ITokenValueProtector
{
    private readonly byte[] _aesKey;
    private readonly byte[] _hmacKey;

    public AesGcmTokenValueProtector(IOptions<PseudonymizationOptions> options)
    {
        var opt = options.Value;
        _aesKey = ResolveKey(opt.TokenMapEncryptionKeyBase64, 32);
        _hmacKey = string.IsNullOrWhiteSpace(opt.TokenMapHmacKeyBase64)
            ? SHA256.HashData(_aesKey)
            : ResolveKey(opt.TokenMapHmacKeyBase64, 32);
    }

    public byte[] Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_aesKey, 16);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var result = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, result, nonce.Length + tag.Length, cipher.Length);
        return result;
    }

    public string Unprotect(byte[] ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        if (ciphertext.Length < 28)
            throw new CryptographicException("Ciphertext too short.");

        var nonce = ciphertext.AsSpan(0, 12);
        var tag = ciphertext.AsSpan(12, 16);
        var cipher = ciphertext.AsSpan(28);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_aesKey, 16);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }

    public string ComputeHmac(string normalizedValue)
    {
        var bytes = Encoding.UTF8.GetBytes(normalizedValue ?? string.Empty);
        var hash = HMACSHA256.HashData(_hmacKey, bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static byte[] ResolveKey(string? base64, int length)
    {
        if (!string.IsNullOrWhiteSpace(base64))
        {
            var decoded = Convert.FromBase64String(base64);
            if (decoded.Length < length)
                throw new InvalidOperationException($"Key must be at least {length} bytes.");
            if (decoded.Length == length)
                return decoded;
            return decoded.AsSpan(0, length).ToArray();
        }

        // Ephemeral key for tests/dev — not durable across process restarts.
        return RandomNumberGenerator.GetBytes(length);
    }
}
