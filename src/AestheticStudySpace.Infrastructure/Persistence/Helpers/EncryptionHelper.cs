using System.Security.Cryptography;
using System.Text;

namespace AestheticStudySpace.Infrastructure.Persistence.Helpers;

public static class EncryptionHelper
{
    // Fixed keys for simple symmetric encryption. Must be 32 bytes (256-bit key) and 16 bytes (128-bit IV) respectively.
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("aEth3ticSpac3Pr0j3ctExe201kEy32"); 
    private static readonly byte[] Iv = Encoding.UTF8.GetBytes("eXe201aEth3tIcIv"); 

    public static string? Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string? Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        catch
        {
            // Fail-safe: if decryption fails (e.g. legacy data was stored in plaintext), return original string
            return cipherText;
        }
    }
}
