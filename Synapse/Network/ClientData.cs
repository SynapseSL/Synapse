using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using EmbedIO;
using EmbedIO.WebApi;
using EmbedIO.WebSockets;
using Synapse.Network.Models;

namespace Synapse.Network
{
    [Serializable]
    public class ClientData
    {
        public string Endpoint { get; set; }
        public string ClientName { get; set; }
        public string ClientUid { get; set; }
        public string CipherKey { get; set; }
        public string ClientCipherKey { get; set; }
        public string SessionToken { get; set; }
        public bool Valid { get; set; }
        public int Port { get; set; }
        public RSA PublicKey { get; set; }

        public HashSet<KeyValueObjectWrapper> SyncEntries { get; set; }
        public string SyncEntriesHash { get; set; }

        public string PublicEndpoint =>
            Endpoint == "127.0.0.1" ? SynapseNetworkServer.GetServer.PublicEndpoint : Endpoint;

        public InstanceDetails ReduceToDetails()
        {
            return new InstanceDetails
            {
                Endpoint = PublicEndpoint,
                Port = Port,
                ClientName = ClientName,
                ClientUid = ClientUid,
                SyncEntries = SyncEntries.ToList()
            };
        }

        public bool ValidateRequest(WebApiController controller)
        {
            return Endpoint == controller.HttpContext.RemoteEndPoint.Address.ToString() && Valid &&
                   controller.Request.Headers.Get("Authorization").Replace("Bearer ", "") == SessionToken
                ? true
                : throw new HttpException(HttpStatusCode.Unauthorized);
        }

        public bool ValidateEndpoint(WebApiController controller)
        {
            return Endpoint == controller.HttpContext.RemoteEndPoint.Address.ToString()
                ? true
                : throw new HttpException(HttpStatusCode.Unauthorized);
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

        public bool ValidateRequestSafe(IWebSocketContext context)
        {
            return Endpoint == context.RemoteEndPoint.Address.ToString() && Valid &&
                   context.Headers.Get("Authorization").Replace("Bearer ", "") == SessionToken;
        }
    }
}