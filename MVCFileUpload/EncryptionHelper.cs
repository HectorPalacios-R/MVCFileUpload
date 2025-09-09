using System.Security.Cryptography;

namespace MVCFileUpload
{
    public class EncryptionHelper
    {
        private static readonly byte[] Key = Convert.FromHexString("1a29afce91f0c876a90f33e2ffb022a4");
        private static readonly byte[] IV = Convert.FromHexString("08C6C4FBAD7C66FDEBF29F75001FD59F");
    
    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();


                }

            }
        }

        public static byte[] Encrypt(byte[] plainBytes)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(plainBytes, encryptor);
                }
            }
        }
        public static byte[] Decrypt(byte[] cipherBytes)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(cipherBytes, encryptor);
                }
            }
        }
    }
}
