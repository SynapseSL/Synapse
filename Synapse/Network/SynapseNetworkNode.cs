using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using Synapse.Api;
using Synapse.Database;
using Synapse.Network.Models;
using Synapse.Network.Routes;

namespace Synapse.Network
{
    public class SynapseNetworkNode : NetworkNodeBase
    {
        private bool _checked;

        public override void RegisterWebserverWith(WebServer server)
        {
            server.WithWebApi("/synapse", x => x.RegisterController<SynapseSynapseRouteController>());
            server.WithWebApi("/networksync", x => x.RegisterController<SynapseNetworkSyncController>());
            server.WithWebApi("/client", x => x.RegisterController<SynapseClientController>());
        }

        public override void Reconfigure(InstanceAuthority authority)
        {
        }

        public async void CheckPings()
        {
            var client = Server.Get.NetworkManager.Client;
            var clients = await GetClients();
            clients.Remove(client.ClientIdentifier);
            foreach (var uid in clients)
            {
                Server.Get.Logger.Warn($"Pinging {uid}");
                var begin = DateTimeOffset.Now;
                var response = await SendMessageAndAwaitResponse(InstanceMessage.CreateSend("Ping", "", uid));
                Server.Get.Logger.Warn("Response Awaited!");
                var offset = DateTimeOffset.Parse(response.Value<string>());
                var delay = offset.Subtract(begin).TotalMilliseconds;
                Server.Get.Logger.Warn($"Ping to {uid} is {delay}ms");
            }
        }

        public override void StartClient(SynapseNetworkClient client)
        {
            //Debug
            //CheckPings();
        }

        public override async void ReceiveInstanceMessage(InstanceMessage message)
        {
            Player local;
            switch (message.Subject)
            {
                case "Echo":
                    Server.Get.Logger.Send($"'{message.Value()}' from {message.Sender}", ConsoleColor.White);
                    break;
                case "Ping":
                    await RespondMessage(message, DateTimeOffset.Now.ToString());
                    break;
                case "SendBroadcast":
                    var broadcast = message.Parse() as NetBroadcast;
                    local = broadcast.Player.ToLocalPlayer();
                    if (local != null)
                    {
                        local.SendBroadcast(broadcast.Duration, broadcast.Message).StartBc(local);
                        await RespondMessage(message, "");
                    }

                    break;
                case "Kick":
                    var kick = message.Parse() as NetKick;
                    local = kick.Player.ToLocalPlayer();
                    if (local != null)
                    {
                        local.Kick(kick.Message);
                        await RespondMessage(message, "");
                    }

                    break;
                case "Ban":
                    var ban = message.Parse() as NetBan;
                    Server.Get.OfflineBanID(ban.Message, "Server", ban.Player.UserId, ban.Duration);
                    await RespondMessage(message, "");
                    break;
                case "GetConfig":
                    Logger.Get.Info("Get config request");
                    var getMessage = message.Parse() as NetConfig;
                    if (getMessage == null) return;
                    string content;
                    switch (getMessage.FileName)
                    {
                        case "config.syml":
                            content = await Server.Get.Files.PermissionFile.ReadFileTextAsync();
                            await RespondMessage(message, new NetConfig
                            {
                                Content = content,
                                FileName = "config.syml"
                            });
                            break;
                        case "permissions.syml":
                            content = await Server.Get.Files.PermissionFile.ReadFileTextAsync();
                            await RespondMessage(message, new NetConfig
                            {
                                Content = content,
                                FileName = "permissions.syml"
                            });
                            break;
                    }

                    break;
                case "SetConfig":
                    var setMessage = message.Parse() as NetConfig;
                    if (setMessage == null) return;
                    switch (setMessage.FileName)
                    {
                        case "config.syml":
                            await Server.Get.Files.ConfigFile.WriteFileTextAsync(setMessage.Content);
                            await RespondMessage(message, "");
                            break;
                        case "permissions.syml":
                            await Server.Get.Files.PermissionFile.WriteFileTextAsync(setMessage.Content);
                            await RespondMessage(message, "");
                            break;
                    }

                    break;
            }

            //Debug
            if (message.Subject == "Ping") RespondMessage(message, DateTimeOffset.Now.ToString());
        }

        public override void Heartbeat()
        {
            if (!_checked)
            {
                _checked = true;
                BroadcastMessage("Echo", "Hello!").GetAwaiter();
                //CheckPings();
            }
        }
    }
}

//Extensions from https://docs.microsoft.com/de-de/dotnet/csharp/programming-guide/concepts/async/using-async-for-file-access
public static class FileExt
{
    public static async Task<string> ReadFileTextAsync(this string filePath)
    {
        using var sourceStream =
            new FileStream(
                filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                4096, true);

        var sb = new StringBuilder();

        var buffer = new byte[0x1000];
        int numRead;
        while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
            var text = Encoding.UTF8.GetString(buffer, 0, numRead);
            sb.Append(text);
        }

        return sb.ToString();
    }

    public static async Task WriteFileTextAsync(this string filePath, string text)
    {
        var encodedText = Encoding.UTF8.GetBytes(text);

        using var sourceStream =
            new FileStream(
                filePath,
                FileMode.Create, FileAccess.Write, FileShare.None,
                4096, true);

        await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
    }
}