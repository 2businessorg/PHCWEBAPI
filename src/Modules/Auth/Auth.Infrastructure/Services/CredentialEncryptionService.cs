using System.Security.Cryptography;
using System.Text;

namespace Auth.Infrastructure.Services;

/// <summary>
/// Service for encrypting and decrypting sensitive data like database credentials
/// Used to protect database credentials stored in JWT claims
/// </summary>
public interface ICredentialEncryptionService
{
    /// <summary>
    /// Encrypts a string value
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted string
    /// </summary>
    string Decrypt(string cipherText);
}

/// <summary>
/// Implementation of credential encryption using AES-256
/// </summary>
public class CredentialEncryptionService : ICredentialEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    /// <summary>
    /// Initialize with encryption key and IV
    /// Key should be 32 bytes (256-bit) for AES-256
    /// IV should be 16 bytes (128-bit)
    /// </summary>
    public CredentialEncryptionService(string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty", nameof(encryptionKey));
        }

        // Create key: use SHA256 hash of the provided key to ensure correct length (32 bytes)
        using (var sha256 = SHA256.Create())
        {
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
        }

        // Create IV: use first 16 bytes of the key hash (or derive from key)
        _iv = new byte[16];
        Array.Copy(_key, _iv, 16);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        var encrypted = ms.ToArray();
                        // Return as Base64 for easy transmission in JWT
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error encrypting data", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try
        {
            var buffer = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(buffer))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error decrypting data", ex);
        }
    }
}
