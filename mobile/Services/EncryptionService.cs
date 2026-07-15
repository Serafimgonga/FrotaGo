using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace FrotaGo.Mobile.Services;

public class EncryptionService
{
    private const string KeyStorage = "frotago_enc_key";

    private async Task<byte[]> GetOrCreateKeyAsync()
    {
        try
        {
            var existing = await SecureStorage.Default.GetAsync(KeyStorage);
            if (!string.IsNullOrEmpty(existing))
            {
                return Convert.FromBase64String(existing);
            }
        }
        catch { }

        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        try
        {
            await SecureStorage.Default.SetAsync(KeyStorage, Convert.ToBase64String(key));
        }
        catch { }
        return key;
    }

    public async Task<byte[]> EncryptAsync(byte[] plaintext)
    {
        var key = await GetOrCreateKeyAsync();
        // AES-GCM: nonce 12 bytes, tag 16 bytes
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[16];
        try
        {
            using var aes = new AesGcm(key);
            aes.Encrypt(nonce, plaintext, cipher, tag);
        }
        catch
        {
            return Array.Empty<byte>();
        }

        var outBytes = new byte[nonce.Length + cipher.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, outBytes, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, outBytes, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag, 0, outBytes, nonce.Length + cipher.Length, tag.Length);
        return outBytes;
    }

    public async Task<byte[]> DecryptAsync(byte[] data)
    {
        var key = await GetOrCreateKeyAsync();
        if (data.Length < 12 + 16) return Array.Empty<byte>();
        var nonce = new byte[12];
        Buffer.BlockCopy(data, 0, nonce, 0, 12);
        var tag = new byte[16];
        var cipherLen = data.Length - 12 - 16;
        var cipher = new byte[cipherLen];
        Buffer.BlockCopy(data, 12, cipher, 0, cipherLen);
        Buffer.BlockCopy(data, 12 + cipherLen, tag, 0, 16);

        var plaintext = new byte[cipherLen];
        try
        {
            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, cipher, tag, plaintext);
            return plaintext;
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }
}
