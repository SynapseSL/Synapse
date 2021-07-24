using System.Collections.Generic;
using System.ComponentModel;
using Synapse.Network;

namespace Synapse.Config
{
    public class SynapseConfiguration : AbstractConfigSection
    {
        #region Hosting
        [Description("If enabled your Server is marked as Synapse Server")]
        public bool NameTracking { get; set; } = true;

        [Description("The default Language that is used for translations")]
        public string Language { get; set; } = "ENGLISH";
        #endregion

        #region GameplayOptions
        [Description("If Enabled FF will be enabled for all Roles at the Round End")]
        public bool AutoFF { get; set; } = true;

        [Description("If disabled Chaos must kill all Scps to end the round")]
        public bool ChaosScpEnd { get; set; } = true;

        [Description("Enables or disables whether the Player needs to equip their keycard to open a door")]
        public bool RemoteKeyCard { get; set; } = false;

        [Description("If Enabled Scp-079 and Scp-096 can't see the Player who is wearing Scp-268")]
        public bool Better268 { get; set; } = false;

        [Description("All roles that can look at 173 without stopping him")]
        public List<int> CantLookAt173 { get; set; } = new List<int>
        {
            (int) RoleType.Tutorial
        };

        [Description("All roles in the list can't rage SCP-096")]
        public List<int> CantRage096 { get; set; } = new List<int>
        {
            (int) RoleType.Tutorial
        };

        [Description("If enabled custom Scps such as 035 can trigger SCP-096's rage")]
        public bool ScpTrigger096 { get; set; } = false;

        [Description("If enabled custom Scps such as 035 can stop SCP-173 by looking at him")]
        public bool ScpTrigger173 { get; set; } = false;

        [Description("The IDs of the Scps which can speak")]
        public List<int> SpeakingScps { get; set; } = new List<int> { 16, 17 };

        [Description("The amount of people needed to contain Scp-106")]
        public int RequiredForFemur { get; set; } = 1;
        #endregion

        #region Messages
        [Description("The duration of how long the TextHint and Broadcast will be displayed")]
        public ushort JoinMessagesDuration { get; set; } = 5;

        [Description("The Broadcast Message a Player gets when joining the Server")]
        public string JoinBroadcast { get; set; } = string.Empty;

        [Description("The TextHint a Player gets when joining the Server")]
        public string JoinTextHint { get; set; } = string.Empty;

        [Description("Opens a Window with the Text when a Player join the Server")]
        public string JoinWindow { get; set; } = string.Empty;
        #endregion

        #region Database
        [Description("Enables or disables the embedded Database. Warning: Disabling this option can break plugins and is not recommended")]
        public bool DatabaseEnabled { get; set; } = true;

        [Description("Changes whether or not the instance should use a shared or an instance specific database")]
        public bool DatabaseShared { get; set; } = true;

        [Description("Enables storing bans in the database persisting them over the network")]
        public bool DatabaseBans { get; set; } = false;
        #endregion

        #region Networking
        [Description("Enables the Networking Capabilites of Synapse")]
        public bool NetworkEnable { get; set; } = false;

        [Description("Enables this server to start a master-webserver if none is present")]
        public bool MasterAuthority { get; set; } = true;

        [Description("The name of the client which will be visible in logs")]
        public string NetworkName { get; set; } = $"SynapseServer-{TokenFactory.Instance.GenerateToken(4)}";

        [Description(
            "The pollrate of the network-client in milliseconds. A lower value result in faster synchronization but increased traffic. Default: 2500")]
        public long NetworkPollRate { get; set; } = 2500;

        [Description("The port used if this server tries to gain master authority")]
        public int NetworkPort { get; set; } = 8880;

        [Description("The shared secret for your synapse server network")]
        public string NetworkSecret { get; set; } = TokenFactory.Instance.GenerateLongToken();

        [Description("The url of master endpoints for the network")]
        public string NetworkUrl { get; set; } = "http://localhost:8880";
        #endregion
    }
}