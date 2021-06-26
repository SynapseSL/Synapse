using Org.BouncyCastle.Utilities.Encoders;
using Synapse.Network;

namespace Synapse.Client.ServerList
{
    public class ServerListInfo
    {
        public int Players { get; set; } = -1;

        public int MaxPlayers { get; set; } = -1;

        public string Info { get; set; } = null;

        public SynapseServerListMark GetMark() => new SynapseServerListMark
        {
            OnlinePlayers = Players < 0 ? ServerConsole.PlayersAmount : Players,
            MaxPlayers = MaxPlayers < 0 ? Server.Get.Slots : MaxPlayers,
            Info = Info == null ? Base64.ToBase64String(ServerConsole.singleton.RefreshServerName().ToBytes()) : Base64.ToBase64String(Info.ToBytes()),
        };
    }
}
