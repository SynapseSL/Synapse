using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan.Formatters;

namespace Synapse.Network
{
    public class SynapseAuthController : WebApiController
    {

        [Route(HttpVerbs.Get, "/ping")]
        public Object Ping()
        {
            var clientData = this.GetClientData();
            return new
            {
                success = true,
                authenticated = clientData != null
            };
        }
        
        
        [Route(HttpVerbs.Post, "/sync")]
        public async Task<NetworkAck> Sync()
        {
            var networkSyn = await HttpContext.GetRequestDataAsync<NetworkSyn>();
            Server.Get.Logger.Info($"Synapse-Network Sync-Request from {networkSyn.ClientName}@{HttpContext.RemoteEndPoint}'");
            ClientData data = new ClientData
            {
                Endpoint = HttpContext.RemoteEndPoint.Address.ToString(),
                PublicKey = RSA.Create(),
                ClientName = networkSyn.ClientName,
                ClientUid = Guid.NewGuid().ToString(),
                SessionToken = TokenFactory.Instance.GenerateShortToken(),
                CipherKey = TokenFactory.Instance.GenerateShortToken(),
                Valid = false
            };
            data.PublicKey.FromXmlString(networkSyn.PublicKey);
            SynapseNetworkServer.Instance.ClientData.Add(data);
            return new NetworkAck
            {
                ClientIdentifier = data.ClientUid,
                PublicKey = SynapseNetworkServer.Instance.PublicKey,
                MigrationPriority = 1
            };
        }

        [Route(HttpVerbs.Post, "/client/{id}/key")]
        public async Task<KeyExchange> ExchangeKeys(string id)
        {
            var data = SynapseNetworkServer.Instance.DataById(id);
            data.ValidateEndpoint(this);
            Server.Get.Logger.Info($"Synapse-Network KeyExchange-Request from {data.ClientName}:{data.ClientUid}@{HttpContext.RemoteEndPoint}");
            var keyExchange = await HttpContext.GetRequestDataAsync<KeyExchange>();
            keyExchange.DecodeWithPrivate(SynapseNetworkServer.Instance.PrivateKey);
            data.ClientCipherKey = keyExchange.Key;

            var ownKeyExchange = new KeyExchange
            {
                Key = data.CipherKey
            };
            ownKeyExchange.EncodeWithPublic(data.PublicKey);
            return ownKeyExchange;
        }

        [Route(HttpVerbs.Post, "/client/{id}/auth")]
        public async Task<string> Authenticate(string id)
        {
            var clientData = SynapseNetworkServer.Instance.DataById(id);
            clientData.ValidateEndpoint(this);
            Server.Get.Logger.Info($"Auth-Request from {clientData.ClientName}:{clientData.ClientUid}@{HttpContext.RemoteEndPoint}");
            var raw = await HttpContext.GetRequestBodyAsStringAsync();
            var content = AESUtils.Decrypt(raw, clientData.CipherKey);
            NetworkReqAuth authReq = Json.Deserialize<NetworkReqAuth>(content);
            if (authReq.ClientIdentifier != clientData.ClientUid)
            {
                Server.Get.Logger.Error($"Auth-Request from {HttpContext.RemoteEndPoint} has invalid ClientId");
                throw new HttpException(HttpStatusCode.Unauthorized);
            }
            if (SynapseNetworkServer.Instance.Secret == authReq.Secret)
            {
                clientData.SessionToken = TokenFactory.Instance.GenerateShortToken();
                clientData.Valid = true;
                Server.Get.Logger.Info($"Synapse-Network Auth-Request from {authReq.ClientIdentifier} successful");
                var responseContent = Json.Serialize(new NetworkResAuth
                {
                    SessionToken = clientData.SessionToken
                });
                
                responseContent = AESUtils.Encrypt(responseContent, clientData.ClientCipherKey);
                return responseContent;
            }

            throw new HttpException(HttpStatusCode.Unauthorized);
        }
    }
}