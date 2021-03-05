using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using EmbedIO;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using JetBrains.Annotations;
using Swan;
using Utf8Json.Resolvers.Internal;

namespace Synapse.Network
{
    public static class SecurityExtensions
    {
        public static string ToHex(this string value)
        {
            return Encoding.UTF8.GetBytes(value).ToLowerHex();
        }
        
        public static string FromHex(this string value)
        {
            return Encoding.UTF8.GetString(value.ConvertHexadecimalToBytes());
        }
        
        public static string ParseString(this byte[] byteArray)
        {
            return Encoding.UTF8.GetString(byteArray);
        }
        
        public static byte[] ToBytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        [CanBeNull]
        public static ClientData GetClientData(this WebApiController controller)
        {
            var authHeader = controller.Request.Headers.Get("Authorization");
            if (authHeader == null) return null;
            var dataClient = SynapseNetworkServer.Instance.DataByToken(authHeader.Replace("Bearer ", ""));
            if (dataClient == null) return null;
            return dataClient.ValidateRequestSafe(controller) ? dataClient : null;
        }

        public static string EncodeStringForClient(this WebApiController controller, string value)
        {
            var authHeader = controller.Request.Headers.Get("Authorization");
            if (authHeader == null) throw new HttpException(HttpStatusCode.Unauthorized);
            var token = authHeader.Replace("Bearer ", "");
            var clientData = SynapseNetworkServer.Instance.DataById(token);
            clientData.ValidateRequest(controller);
            return AESUtils.Encrypt(value, clientData.ClientCipherKey);
        }

        public static string DecodeStringFromClient(this WebApiController controller, string value)
        {
            var authHeader = controller.Request.Headers.Get("Authorization");
            if (authHeader == null) throw new HttpException(HttpStatusCode.Unauthorized);
            var token = authHeader.Replace("Bearer ", "");
            var clientData = SynapseNetworkServer.Instance.DataById(token);
            clientData.ValidateRequest(controller);
            return AESUtils.Decrypt(value, clientData.CipherKey);
        }
        
        public static string EncodeStringForServer(this SynapseNetworkClient client, string value)
        {
            return AESUtils.Encrypt(value, client.ServerCipherKey);
        }
        
         
        public static string DecodeStringFromServer(this SynapseNetworkClient client, string value)
        {
            return AESUtils.Decrypt(value, client.CipherKey);
        }
    }
}