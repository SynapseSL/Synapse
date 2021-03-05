using System;
using System.Security.Cryptography;
using Swan;

namespace Synapse.Network
{
    [Serializable]
    public class KeyExchange
    {
        public string Key { get; set; }

        public void EncodeWithPublic(RSA rsa)
        {
            Key = rsa.Encrypt(Key.ToBytes(), RSAEncryptionPadding.Pkcs1).ToLowerHex();
        }
        
        public void DecodeWithPrivate(RSA rsa)
        {
            Key = rsa.Decrypt(Key.ConvertHexadecimalToBytes(), RSAEncryptionPadding.Pkcs1)
                .ParseString();
        }
    }
}