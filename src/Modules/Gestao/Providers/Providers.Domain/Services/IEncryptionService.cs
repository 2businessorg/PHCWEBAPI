namespace Providers.Domain.Services;

/// <summary>
/// Interface para serviço de encriptação/desencriptação
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
}
