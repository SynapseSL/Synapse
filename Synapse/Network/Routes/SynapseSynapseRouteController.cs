using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan.Formatters;
using Synapse.Network.Models;

namespace Synapse.Network.Routes
{
    public class SynapseSynapseRouteController : WebApiController
    {
        [Route(HttpVerbs.Get, "/ping")]
        public StatusedResponse Ping()
        {
            var clientData = this.GetClientData();
            if (clientData != null)
                SynapseNetworkServer.GetServer.SyncedClientList[clientData.ClientUid] = DateTimeOffset.Now;

            return new PingResponse
            {
                Authenticated = clientData != null,
                ConnectedClients = clientData == null
                    ? new List<string>()
                    : SynapseNetworkServer.GetServer.SyncedClientList.Keys.ToList(),
                Messages = clientData == null
                    ? new List<InstanceMessage>()
                    : SynapseNetworkServer.GetServer.TakeAllMessages(clientData),
                LatestVarHash = clientData == null ? "\r\n" : clientData.SyncEntriesHash
            };
        }

        [Route(HttpVerbs.Get, "/clients")]
        public StatusedResponse Clients()
        {
            var clientData = this.GetClientData();
            if (clientData == null) return StatusedResponse.Unauthorized;
            return new StatusListWrapper<string>(SynapseNetworkServer.GetServer.SyncedClientList.Keys);
        }


        [Route(HttpVerbs.Post, "/post")]
        public async Task<StatusedResponse> Post()
        {
            var clientData = this.GetClientData();
            if (clientData == null) return StatusedResponse.Unauthorized;
            var msg = await HttpContext.GetRequestDataAsync<InstanceMessage>();
            if (msg.Receiver == "@")
            {
                var recv = new List<string>();
                foreach (var target in SynapseNetworkServer.GetServer.TokenClientIDMap.Values.Where(x =>
                    x != clientData.ClientUid)
                )
                {
                    recv.Add(target);
                    SynapseNetworkServer.GetServer.AddMessage(target, msg);
                }

                return new InstanceMessageTransmission
                {
                    Receivers = recv
                };
            }
            else
            {
                var results = SynapseNetworkServer.GetServer.TokenClientIDMap.Values.Where(x => x == msg.Receiver);
                if (!results.Any())
                    return new InstanceMessageTransmission
                    {
                        Receivers = new List<string>()
                    };
                var recv = results.First();
                SynapseNetworkServer.GetServer.AddMessage(recv, msg);
                return new InstanceMessageTransmission
                {
                    Receivers = new[] {recv}.ToList()
                };
            }
        }


        [Route(HttpVerbs.Post, "/handshake")]
        public async Task<StatusedResponse> Handshake()
        {
            var networkSyn = await HttpContext.GetRequestDataAsync<NetworkAuthSyn>();
            Server.Get.Logger.Info(
                $"Synapse-Network Handshake-Request from {networkSyn.ClientName}@{HttpContext.RemoteEndPoint}'");
            var data = new ClientData
            {
                Endpoint = HttpContext.RemoteEndPoint.Address.ToString(),
                PublicKey = RSA.Create(),
                ClientName = networkSyn.ClientName,
                Port = networkSyn.Port,
                ClientUid = Guid.NewGuid().ToString(),
                SessionToken = TokenFactory.Instance.GenerateShortToken(),
                CipherKey = TokenFactory.Instance.GenerateShortToken(),
                SyncEntriesHash = "\r\n",
                SyncEntries = new HashSet<KeyValueObjectWrapper>(),
                Valid = false
            };
            data.PublicKey.FromXmlString(networkSyn.PublicKey);
            SynapseNetworkServer.GetServer.AddClient(data);
            Server.Get.Logger.Info("Synapse-Network Client to Cache'");
            return new NetworkAuthAck
            {
                ClientIdentifier = data.ClientUid,
                PublicKey = SynapseNetworkServer.GetServer.PublicKey,
                MigrationPriority = 1
            };
        }

        [Route(HttpVerbs.Get, "/client/{id}/details")]
        public StatusedResponse GetDetails(string id)
        {
            var data = this.GetClientData();
            if (data == null) return StatusedResponse.Unauthorized;
            if (id == "all")
            {
                var wrapper = new InstanceDetailsListTransmission
                {
                    Details = SynapseNetworkServer.GetServer.AllClientData().Select(x => x.ReduceToDetails()).ToArray()
                };
                return wrapper;
            }

            var target = SynapseNetworkServer.GetServer.DataById(id);
            if (target == null) return StatusedResponse.NotFound;
            return new InstanceDetailsTransmission
            {
                Details = target.ReduceToDetails()
            };
        }

        [Route(HttpVerbs.Post, "/client/{id}/key")]
        public async Task<StatusedResponse> ExchangeKeys(string id)
        {
            var data = SynapseNetworkServer.GetServer.DataById(id);
            data.ValidateEndpoint(this);
            Server.Get.Logger.Info(
                $"Synapse-Network KeyExchange-Request from {data.ClientName}:{data.ClientUid}@{HttpContext.RemoteEndPoint}");
            var keyExchange = await HttpContext.GetRequestDataAsync<NetworkAuthKeyExchange>();
            keyExchange.DecodeWithPrivate(SynapseNetworkServer.GetServer.PrivateKey);
            data.ClientCipherKey = keyExchange.Key;

            var ownKeyExchange = new NetworkAuthKeyExchange
            {
                Key = data.CipherKey
            };
            ownKeyExchange.EncodeWithPublic(data.PublicKey);
            return ownKeyExchange;
        }

        [Route(HttpVerbs.Post, "/client/{id}/auth")]
        public async Task<string> Authenticate(string id)
        {
            var clientData = SynapseNetworkServer.GetServer.DataById(id);
            clientData.ValidateEndpoint(this);
            Server.Get.Logger.Info(
                $"Auth-Request from {clientData.ClientName}:{clientData.ClientUid}@{HttpContext.RemoteEndPoint}");
            var raw = await HttpContext.GetRequestBodyAsStringAsync();
            var content = AESUtils.Decrypt(raw, clientData.CipherKey);
            var authAuthReq = Json.Deserialize<NetworkAuthReqAuth>(content);
            if (authAuthReq.ClientIdentifier != clientData.ClientUid)
            {
                Server.Get.Logger.Error($"Auth-Request from {HttpContext.RemoteEndPoint} has invalid ClientId");
                throw new HttpException(HttpStatusCode.Unauthorized);
            }

            if (SynapseNetworkServer.GetServer.Secret == authAuthReq.Secret)
            {
                clientData.SessionToken = TokenFactory.Instance.GenerateShortToken();
                Server.Get.NetworkManager.Server.TokenClientIDMap[clientData.SessionToken] = clientData.ClientUid;
                clientData.Valid = true;
                Server.Get.Logger.Info($"Synapse-Network Auth-Request from {authAuthReq.ClientIdentifier} successful");
                var responseContent = Json.Serialize(new NetworkAuthResAuth
                {
                    SessionToken = clientData.SessionToken
                });
                responseContent = AESUtils.Encrypt(responseContent, clientData.ClientCipherKey);
                SynapseNetworkServer.GetServer.SyncedClientList[clientData.ClientUid] = DateTimeOffset.Now;
                return responseContent;
            }

            throw new HttpException(HttpStatusCode.Unauthorized);
        }
    }
}