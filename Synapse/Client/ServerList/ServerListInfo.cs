using Org.BouncyCastle.Utilities.Encoders;
using Synapse.Network;

namespace Synapse.Client.ServerList
{
    public class ServerListInfo
    {
        /// <summary>
        /// The amount of players that should be displayed on the server list
        /// </summary>
        /// <remarks>
        /// Set it to -1 for using current player amount
        /// </remarks>
        public int Players { get; set; } = -1;

        /// <summary>
        /// The max amount of players that can join the server displayed on the server list
        /// </summary>
        /// <remarks>
        /// Set it to -1 for using the activated slot amount
        /// </remarks>
        public int MaxPlayers { get; set; } = -1;

        /// <summary>
        /// The name of the server displayed on the server list
        /// </summary>
        /// <remarks>
        /// Set it to null for using the server name in the config
        /// </remarks>
        public string Info { get; set; } = null;

        /// <summary>
        /// Calculates which Values should be used on the server list
        /// </summary>
        /// <returns><see cref="SynapseServerListMark"/></returns>
        public SynapseServerListMark GetMark() => new SynapseServerListMark
        {
            OnlinePlayers = Players < 0 ? ServerConsole.PlayersAmount : Players,
            MaxPlayers = MaxPlayers < 0 ? Server.Get.Slots : MaxPlayers,
            Info = Info == null ? Base64.ToBase64String(ServerConsole.singleton.RefreshServerName().ToBytes()) : Base64.ToBase64String(Info.ToBytes()),
        };
    }
}
