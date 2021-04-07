using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Synapse.Network.Models
{
    public class ClientSession
    {
        public string Token { get; set; }
        public string InCipher { get; set; }
        public string OutCipher { get; set; }

        public string Encode(string body)
        {
            return AESUtils.Encrypt(body, OutCipher);
        }

        public string Decode(string body)
        {
            return AESUtils.Decrypt(body, InCipher);
        }

        [CanBeNull]
        public static ClientSession Validate(string hexUser, string encToken, out string userOut, string permission = null)
        {
            userOut = null;
            var user = hexUser.FromHex();
            var exists = Server.Get.NetworkManager.ClientSessionTokens.TryGetValue(user, out var session);
            if (!exists) return null;
            var token = AESUtils.Decrypt(encToken, session.InCipher);
            if (token != session.Token) return null;
            userOut = user;
            if (permission != null)
            {
                var group = Server.Get.PermissionHandler.GetPlayerGroup(user);
                if (!group.HasPermission(permission)) return null;
            }
            
            return session;
        }
    }
}