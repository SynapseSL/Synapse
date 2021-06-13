using System;
using Swan;

namespace Synapse.Network
{
    public class TokenFactory
    {
        
        private static TokenFactory _instance;
        public static TokenFactory Instance => _instance ?? new TokenFactory();

        private readonly Random _random = new Random();

        private TokenFactory()
        {
            _instance = this;
        }
        
        public string GenerateShortToken()
        {
            byte[] key = new byte[32];
            _random.NextBytes(key);
            return key.ToLowerHex();
        }
        
        public string GenerateLongToken()
        {
            byte[] key = new byte[64];
            _random.NextBytes(key);
            return key.ToLowerHex();
        }
        
        public byte[] Token32()
        {
            byte[] key = new byte[32];
            _random.NextBytes(key);
            return key;
        }
        
        public byte[] Token64()
        {
            byte[] key = new byte[64];
            _random.NextBytes(key);
            return key;
        }
        
        public string GenerateToken(int bytes = 32)
        {
            byte[] key = new byte[bytes];
            _random.NextBytes(key);
            return key.ToLowerHex();
        }

    }
}