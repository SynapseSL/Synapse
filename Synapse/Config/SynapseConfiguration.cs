using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Synapse.Config
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    public class SynapseConfiguration : AbstractConfigSection
    {
        [Description("Enables or disables the embedded Database. Warning: Disabling this option can break plugins and is not recommended")]
        public bool DatabaseEnabled = true;

        [Description("Changes whether or not the instance should use a shared or an instance specific database")]
        public bool DatabaseShared = true;
        
        [Description("Enables or disables whether the Player needs to equip their keycard to open a door")]
        public bool RemoteKeyCard = false;

        [Description("The Broadcast Message a Player gets when joining the Server")]
        public string JoinBroadcast = string.Empty;

        [Description("The TextHint a Player gets when joining the Server")]
        public string JoinTextHint = string.Empty;

        [Description("Opens a Window with the Text when a Player join the Server")]
        public string JoinWindow = string.Empty;

        [Description("The duration of how long the TextHint and Broadcast will be displayed")]
        public ushort JoinMessagesDuration = 5;

        [Description("The IDs of the Scps which can speak")]
        public List<int> SpeakingScps = new List<int> { 16, 17 };

        [Description("If enabled your Server is marked as Synapse Server")]
        public bool NameTracking = true;

        [Description("The amount of people needed to contain Scp-106")]
        public int RequiredForFemur = 1;

        [Description("If disabled Chaos must kill all Scps to end the round")]
        public bool ChaosScpEnd = true;

        [Description("If enabled custom Scps such as 035 can trigger Scp096's rage")]
        public bool ScpTrigger096 = false;

        [Description("If Enabled Scp-079 and Scp-096 can't see the Player who is wearing Scp-268")]
        public bool Better268 = false;

        [Description("All roles in the list can't rage SCP-096")]
        public List<int> CantRage096 = new List<int>
        {
            (int)RoleType.Tutorial
        };

        [Description("The default Language that is used for translations")]
        public string Language = "ENGLISH";
    }
}