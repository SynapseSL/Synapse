using System;
using System.Net;
using System.Security.Cryptography;
using EmbedIO;
using EmbedIO.WebApi;

namespace Synapse.Network
{
    [Serializable]
    public class ClientData
    {
        public string Endpoint;
        public string ClientName;
        public string ClientUid;
        public RSA PublicKey;
        public string CipherKey;
        public string ClientCipherKey;
        public string SessionToken;
        public bool Valid;

        public bool ValidateRequest(WebApiController controller)
        {
            return Endpoint == controller.HttpContext.RemoteEndPoint.Address.ToString() && Valid &&
                   controller.Request.Headers.Get("Authorization").Replace("Bearer ", "") == SessionToken
                ? true
                : throw new HttpException(HttpStatusCode.Unauthorized);
        }
        
        public bool ValidateEndpoint(WebApiController controller)
        {
            return Endpoint == controller.HttpContext.RemoteEndPoint.Address.ToString() ? true : throw new HttpException(HttpStatusCode.Unauthorized);
        }
        
        public bool ValidateRequestSafe(WebApiController controller)
        {
            return Endpoint == controller.HttpContext.RemoteEndPoint.Address.ToString() && Valid &&
                   controller.Request.Headers.Get("Authorization").Replace("Bearer ", "") == SessionToken;
        }
        
        public bool ValidateEndpointSafe(WebApiController controller)
        {
            return Endpoint == controller.HttpContext.RemoteEndPoint.Address.ToString();
        }
    }
}