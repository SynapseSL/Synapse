using System;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan;
using Swan.Formatters;
using Synapse.Database;
using Synapse.Network.Models;

namespace Synapse.Network.Routes
{
    public class SynapseClientController : WebApiController
    {
        [Route(HttpVerbs.Post, "/login")]
        public async Task<IStatus> Login([QueryField("user", true)] string encodedUser)
        {
            var user = encodedUser.FromHex();
            var body = await HttpContext.GetRequestBodyAsStringAsync();
            var dbo = DatabaseManager.PlayerRepository.FindByGameId(user);
            var exists = dbo.Data.TryGetValue("netpass", out var password);
            if (!exists) return StatusedResponse.Unauthorized;
            var decryptedBody = AESUtils.Decrypt(body, password);
            if (!decryptedBody.Trim().StartsWith("{")) return StatusedResponse.Unauthorized;
            var loginRequest = Json.Deserialize<ClientLoginRequest>(decryptedBody);
            if (loginRequest.ClientSecret != password || loginRequest.ClientUid != user)
                return StatusedResponse.Unauthorized;
            var token = TokenFactory.Instance.GenerateShortToken();
            var responseCipher = TokenFactory.Instance.GenerateShortToken();
            Server.Get.NetworkManager.ClientSessionTokens[user] = new ClientSession
            {
                InCipher = loginRequest.ResponseCipher,
                OutCipher = responseCipher,
                Token = token
            };
            var encodedToken = AESUtils.Encrypt(token, loginRequest.ResponseCipher);
            var encodedResponseCipher = AESUtils.Encrypt(responseCipher, loginRequest.ResponseCipher);
            return new ClientLoginResponse
            {
                ClientToken = encodedToken,
                ResponseCipher = encodedResponseCipher
            };
        }

        [Route(HttpVerbs.Get, "/servers")]
        public async Task<object> Servers([QueryField("user", true)] string encodedUser,
            [QueryField("token", true)] string encodedToken)
        {
            var session = ClientSession.Validate(encodedUser, encodedToken, out var user);
            if (session == null) return StatusedResponse.Unauthorized;
            var body = Json.Serialize(new InstanceDetailsListTransmission
            {
                Details = SynapseNetworkServer.GetServer.AllClientData().Select(x => x.ReduceToDetails()).ToArray()
            });
            return session.Encode(body);
        }

        [Route(HttpVerbs.Get, "/config")]
        public async Task<object> Config(
            [QueryField("user", true)] string encodedUser,
            [QueryField("token", true)] string encodedToken,
            [QueryField("target", true)] string target,
            [QueryField("file", true)] string file)
        {
            var session = ClientSession.Validate(encodedUser, encodedToken, out var user);
            if (session == null) return StatusedResponse.Unauthorized;
            var response = await SynapseNetworkClient.GetClient.SendMessageAndAwaitResponse(InstanceMessage.CreateSend(
                "GetConfig",
                new NetConfig
                {
                    Content = "",
                    FileName = file
                }, target));
            var cfg = response.Parse() as NetConfig;
            return session.Encode(cfg.ToJson());
        }

        [Route(HttpVerbs.Post, "/config")]
        public async Task<object> SetConfig(
            [QueryField("user", true)] string encodedUser,
            [QueryField("token", true)] string encodedToken,
            [QueryField("target", true)] string target,
            [QueryField("file", true)] string file /* Reserved so future updates won't break possible PWAs */)
        {
            var session = ClientSession.Validate(encodedUser, encodedToken, out var user);
            if (session == null) return StatusedResponse.Unauthorized;
            var content = await HttpContext.GetRequestBodyAsStringAsync();
            content = session.Decode(content);
            var netConfig = Json.Deserialize<NetConfig>(content);
            await SynapseNetworkClient.GetClient.SendMessageAndAwaitResponse(InstanceMessage.CreateSend("SetConfig",
                netConfig, target));
            return new StatusedResponse
            {
                Successful = true,
                Message = "Updated"
            };
        }
        
        [Route(HttpVerbs.Post, "/ban")]
        public async Task<object> Ban(
            [QueryField("user", true)] string encodedUser,
            [QueryField("token", true)] string encodedToken)
        {
            var session = ClientSession.Validate(encodedUser, encodedToken, out var user);
            if (session == null) return StatusedResponse.Unauthorized;
            var content = await HttpContext.GetRequestBodyAsStringAsync();
            content = session.Decode(content);
            var netBan = Json.Deserialize<NetBan>(content);
            await netBan.Player.Ban(netBan.Message, user, netBan.Duration);
            return new StatusedResponse
            {
                Successful = true,
                Message = "Ok"
            };
        }
    }
}