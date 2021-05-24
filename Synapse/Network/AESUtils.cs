using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Swan;
using Synapse.Api;

namespace Synapse.Network
{
    /// <summary>
    ///     Utility for dealing with AES
    ///     Credits to Troy Alford (https://stackoverflow.com/users/1454806/troy-alford) for
    ///     providing this example at https://stackoverflow.com/questions/273452/using-aes-encryption-in-c-sharp
    /// </summary>
    public static class AESUtils
    {
        public static string Encrypt(string value, string password)
        {
            return Encrypt<AesManaged>(value, password);
        }

        public static string Encrypt<T>(string value, string password)
            where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = _vector.ToBytes();
            var saltBytes = _salt.ToBytes();
            var valueBytes = value.ToBytes();

            byte[] encrypted;
            using (var cipher = new T())
            {
                var _passwordBytes = new Rfc2898DeriveBytes(password, saltBytes, _iterations);
                var keyBytes = _passwordBytes.GetBytes(_keySize / 8);
                cipher.Mode = CipherMode.CBC;

                using (var encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                {
                    using var to = new MemoryStream();
                    using var writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write);
                    writer.Write(valueBytes, 0, valueBytes.Length);
                    writer.FlushFinalBlock();
                    encrypted = to.ToArray();
                }

                cipher.Clear();
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string value, string password)
        {
            return Decrypt<AesManaged>(value, password);
        }

        public static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = _vector.ToBytes();
            var saltBytes = _salt.ToBytes();
            var valueBytes = Convert.FromBase64String(value);

            byte[] decrypted;
            var decryptedByteCount = 0;

            using (var cipher = new T())
            {
                var _passwordBytes = new Rfc2898DeriveBytes(password, saltBytes, _iterations);
                var keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                Logger.Get.Error(keyBytes.ToLowerHex());

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using var decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes);
                    using var from = new MemoryStream(valueBytes);
                    using var reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read);
                    decrypted = new byte[valueBytes.Length];
                    decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                }
                catch
                {
                    return string.Empty;
                }

                cipher.Clear();
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }

        #region Settings

        private static readonly int _iterations = 2;
        private static readonly int _keySize = 256;

        // I dont care about if or if not the salt & vector are secure since I am just
        // using this cipher is short lived and the key is encrypted with RSA.
        // But I will make them session generated at a later point ~Helight
        private readonly static string _hash = "SHA1";
        private static readonly string _salt = "aselrias38490a32"; // Static
        private static readonly string _vector = "8947az34awl34kjq"; // Static

        #endregion
    }
}