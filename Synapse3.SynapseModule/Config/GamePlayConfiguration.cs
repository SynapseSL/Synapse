using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all GamePlay related stuff
/// </summary>
[DocumentSection("GamePlay")]
public class GamePlayConfiguration : IDocumentSection
{
    [Description("If Enabled everyone can attack everyone after the Round ended")]
    public bool AutoFriendlyFire = true;

    [Description("If Enabled SCP-268 will hide you even for SCP-096 and SCP-079")]
    public bool BetterScp268 = false;

    [Description("If enabled a Player don't need to equip his keycard to use it")]
    public bool RemoteKeyCard = false;
}