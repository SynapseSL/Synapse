using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Synapse.Api;
using Synapse.Database;
using Synapse.Network.Models;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkPlayer
    {
        public string Endpoint { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public string CurrentServerId { get; set; }

        [CanBeNull]
        public Player ToLocalPlayer()
        {
            var results = Server.Get.GetPlayers(x => x.UserId == UserId);
            return results.IsEmpty() ? null : results.First();
        }

        public async Task SendBroadcastMessage(string message)
        {
            var local = ToLocalPlayer();
            if (local != null)
            {
                local.SendBroadcast(5, message).StartBc(local);
                return;
            }

            await SynapseNetworkClient.GetClient.SendMessageAndAwaitResponse(InstanceMessage.CreateBroadcast(
                "SendBroadcast", new NetBroadcast
                {
                    Player = this,
                    Message = message
                }));
        }

        public async Task Kick(string message, string issuer = null, string note = null, bool log = true)
        {

            if (PunishmentRepository.Enabled)
            {
                PunishmentRepository.CreateKick(UserId, message, issuer ?? "Server", DisplayName ?? "Unknown", note ?? "");
            }
            
            var local = ToLocalPlayer();
            if (local != null)
            {
                
                local.Kick(message);
                return;
            }

            await SynapseNetworkClient.GetClient.SendMessageAndAwaitResponse(InstanceMessage.CreateBroadcast("Kick",
                new NetKick
                {
                    Player = this,
                    Message = message,
                    Issuer = issuer,
                    Note = note
                }));
        }

        //Durations is in seconds
        public async Task Ban(string message, string issuer, int duration, string note = "")
        {
            if (PunishmentRepository.Enabled)
            {
                PunishmentRepository.CreateBan(UserId, message, issuer??"Server", duration, DisplayName??"Unknown Name", note);
                await Kick(message);
            }
            else
            {
                await SynapseNetworkClient.GetClient.SendMessageAndAwaitResponse(InstanceMessage.CreateBroadcast("Ban",
                    new NetBan
                    {
                        Player = this,
                        Message = message,
                        Duration = duration,
                        Note = note
                    })); 
            }
        }

        public static NetworkPlayer FromLocalPlayer(Player player)
        {
            return new NetworkPlayer
            {
                Endpoint = player.IpAddress,
                DisplayName = player.DisplayName,
                UserId = player.UserId,
                RoleName = player.RoleName,
                CurrentServerId = SynapseNetworkClient.GetClient.ClientIdentifier
            };
        }

        public static NetworkPlayer FromUID(string uid)
        {
            return new NetworkPlayer
            {
                Endpoint = "null",
                DisplayName = "null",
                UserId = uid,
                RoleName = "None",
                CurrentServerId = "null"
            };
        }
    }
}