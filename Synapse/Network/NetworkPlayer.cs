using System;
using System.Linq;
using JetBrains.Annotations;
using Synapse.Api;

namespace Synapse.Network
{
    [Serializable]
    public class NetworkPlayer
    {
        public string Endpoint { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public string RoleName { get; set; }

        [CanBeNull]
        public Player ToLocalPlayer()
        {
            var results = Server.Get.GetPlayers(x => x.UserId == UserId);
            return results.IsEmpty() ? null : results.First();
        }

        public static NetworkPlayer FromLocalPlayer(Player player)
        {
            return new NetworkPlayer
            {
                Endpoint = player.IpAddress,
                DisplayName = player.DisplayName,
                UserId = player.UserId,
                RoleName = player.RoleName
            };
        }
    }
}