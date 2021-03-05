using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO;
using JetBrains.Annotations;
using Mirror;
using Swan;
using Swan.Formatters;

namespace Synapse.Network
{
    public class SynapseNetworkClient
    {
        private HttpClient Client;

        public string Url;
        public string SessionToken;
        public string Secret;
        public string ClientName = "SynapseServerClient";
        public string ClientIdentifier;
        public RSA PrivateKey;
        public RSA ServerPublicKey;
        public string CipherKey;
        public string ServerCipherKey;
        public string PublicKey;
        public int MigrationPriority;

        public bool IsStarted = false;

        public void Init()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(Url);
            PrivateKey = RSA.Create();
            CipherKey = TokenFactory.Instance.GenerateShortToken();
            PublicKey = PrivateKey.ToXmlString(false);
        }
        
        [ItemCanBeNull]
        public async Task<TR> Get<TR>(string path, bool authenticated = true, Action<Exception> exceptionHandler = null)
        {
            var exc = exceptionHandler ?? (x => Server.Get.Logger.Error(x));
            try
            {
                var content = await GetString(path, authenticated, exc);
                return Json.Deserialize<TR>(content);
            }
            catch (Exception e)
            {
                exc(e);
                return (TR) await Task.FromResult<object>(null);
            }
        }

        [ItemCanBeNull]
        public async Task<string> GetString(string path, bool authenticated = true, Action<Exception> exceptionHandler = null)
        {
            var exc = exceptionHandler ?? (x => Server.Get.Logger.Error(x));
            try
            {
                var message = new HttpRequestMessage();
                message.RequestUri = new Uri(Url + path);
                message.Method = HttpMethod.Get;
                if (authenticated) message.Headers.Authorization = new AuthenticationHeaderValue("Bearer",SessionToken);
                var result = await Client.SendAsync(message);
                var content = await result.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception e)
            {
                exc(e);
                return (string) await Task.FromResult<object>(null);
            }
        }
        
        [ItemCanBeNull]
        public async Task<string> PostString<TB>(string path, TB body, bool authenticated = true, bool encodeBody = true, Action<Exception> exceptionHandler = null)
        {
            // ReSharper disable once HeapView.PossibleBoxingAllocation
            var exc = exceptionHandler ?? (x => Server.Get.Logger.Error(x));
            try
            {
                var data = encodeBody ? Json.Serialize(body) : body.ToString();
                var message = new HttpRequestMessage();
                message.RequestUri = new Uri(Url + path);
                message.Method = HttpMethod.Post;
                message.Content = new StringContent(data);
                if (authenticated) message.Headers.Authorization = new AuthenticationHeaderValue("Bearer",SessionToken);
                var result = await Client.SendAsync(message);
                var content = await result.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception e)
            {
                exc(e);
                return (string) await Task.FromResult<object>(null);
            }
        }
        
        [ItemCanBeNull]
        public async Task<TR> Post<TR,TB>(string path, TB body, bool authenticated = true, bool encodeBody = true, Action<Exception> exceptionHandler = null)
        {
            // ReSharper disable once HeapView.PossibleBoxingAllocation
            var exc = exceptionHandler ?? (x => Server.Get.Logger.Error(x));
            try
            {
                var content = await PostString(path, body, authenticated, encodeBody, exc);
                return Json.Deserialize<TR>(content);
            }
            catch (Exception e)
            {
                exc(e);
                return (TR) await Task.FromResult<object>(null);
            }
        }

        public async Task<bool> CheckAvailability()
        {
            try
            {
                var result = await Get<Dictionary<string,object>>("/synapse/ping", exceptionHandler: x => {});
#if DEBUG
                Server.Get.Logger.Info($"Ping Result: {result?.Humanize().Insert(0, "\n")??"connection reset"}");
#endif
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void OnConnected()
        {
            var networkNodes = Server.Get.NetworkManager.NetworkNodes;
            networkNodes.ForEach(x => x.StartClient(this));
            var authority = SynapseNetworkServer.Instance.Status == WebServerState.Stopped ? InstanceAuthority.Client : InstanceAuthority.Master;
            networkNodes.ForEach(x => x.Reconfigure(authority));
        }
        
        public async void Connect()
        {
            IsStarted = true;
            try
            {
                Server.Get.Logger.Info("Connecting to Synapse-Network...");
                await SyncMaster();
                await KeyExchange();
                await AuthMaster();
                Server.Get.Logger.Info($"Connected to Master-Server with MigrationPriority {MigrationPriority} and ClientUID{ClientIdentifier}");
                Server.Get.Logger.Send($"Synapse-Network Session-Token is {SessionToken}", ConsoleColor.Magenta);
                OnConnected();
            }
            catch (Exception e)
            {
                IsStarted = false;
                Server.Get.Logger.Error(e);
            }
        }

        public async void Disconnect()
        {
            //TODO
        }


        private async Task KeyExchange()
        {
            var obj = new KeyExchange
            {
                Key = CipherKey
            };
            obj.EncodeWithPublic(ServerPublicKey);
            var reqContent = Json.Serialize(obj);
            var req = await Client.PostAsync($"/synapse/client/{ClientIdentifier}/key", new StringContent(reqContent));
            var content = await req.Content.ReadAsStringAsync();
            var serverKey = Json.Deserialize<KeyExchange>(content);
            serverKey.DecodeWithPrivate(PrivateKey);
            ServerCipherKey = serverKey.Key;
        }

        private async Task SyncMaster()
        {
            var obj = new NetworkSyn
            {
                ClientName = ClientName,
                PublicKey = PublicKey
            };
            var reqContent = Json.Serialize(obj);
            var req = await Client.PostAsync("/synapse/sync", new StringContent(reqContent));
            var content = await req.Content.ReadAsStringAsync();
            var ack = Json.Deserialize<NetworkAck>(content);
            ServerPublicKey = RSA.Create();
            ServerPublicKey.FromXmlString(ack.PublicKey);
            MigrationPriority = ack.MigrationPriority;
            ClientIdentifier = ack.ClientIdentifier;
        }

        private async Task AuthMaster()
        {
            var payload = Json.Serialize(new NetworkReqAuth
            {
                Secret = Secret,
                ClientIdentifier = ClientIdentifier
            });
            payload = AESUtils.Encrypt(payload, ServerCipherKey);
            var req = await Client.PostAsync($"/synapse/client/{ClientIdentifier}/auth", new StringContent(payload));
            var content = await req.Content.ReadAsStringAsync();
            content = content.Replace("\"", ""); //For any reason, this payload always is in " ".
            content = AESUtils.Decrypt(content, CipherKey);
            var res = Json.Deserialize<NetworkResAuth>(content);
            SessionToken = res.SessionToken;
        }
        
    }
}