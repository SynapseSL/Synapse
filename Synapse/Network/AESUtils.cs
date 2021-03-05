using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Synapse.Network
{
    /// <summary>
    /// Utility for dealing with AES
    ///
    /// Credits to Troy Alford (https://stackoverflow.com/users/1454806/troy-alford) for
    /// providing this example at https://stackoverflow.com/questions/273452/using-aes-encryption-in-c-sharp
    /// </summary>
    public static class AESUtils {
        #region Settings

        private static int _iterations = 2;
        private static int _keySize = 256;

        // I dont care about if or if not the salt & vector are secure since I am just
        // using this cipher is short lived and the key is encrypted with RSA.
        // But I will make them session generated at a later point ~Helight
        private static string _hash     = "SHA1";
        private static string _salt     = "aselrias38490a32"; // Static
        private static string _vector   = "8947az34awl34kjq"; // Static

        #endregion

        public static string Encrypt(string value, string password) {
            return Encrypt<AesManaged>(value, password);
        }
        public static string Encrypt<T>(string value, string password) 
            where T : SymmetricAlgorithm, new() {
            byte[] vectorBytes = _vector.ToBytes();
            byte[] saltBytes = _salt.ToBytes();
            byte[] valueBytes = value.ToBytes();

            byte[] encrypted;
            using (T cipher = new T()) {
                PasswordDeriveBytes _passwordBytes = 
                    new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes)) {
                    using (MemoryStream to = new MemoryStream()) {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write)) {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                cipher.Clear();
            }
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string value, string password) {
            return Decrypt<AesManaged>(value, password);
        }
        public static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            byte[] vectorBytes = _vector.ToBytes();
            byte[] saltBytes = _salt.ToBytes();
            byte[] valueBytes = Convert.FromBase64String(value);

            byte[] decrypted;
            int decryptedByteCount = 0;

            using (T cipher = new T()) {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes)) {
                        using (MemoryStream from = new MemoryStream(valueBytes)) {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read)) {
                                decrypted = new byte[valueBytes.Length];
                                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    }
                } catch (Exception ex) {
                    return String.Empty;
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }

    }
}