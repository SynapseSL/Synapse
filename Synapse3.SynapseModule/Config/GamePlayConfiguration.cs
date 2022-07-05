using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

[DocumentSection("GamePlay")]
public class GamePlayConfiguration : IDocumentSection
{
    [Description("If Enabled everyone can attack everyone after the Round ended")]
    public bool AutoFriendlyFire = true;

    [Description("If Enabled SCP-268 will hide you even for SCP-096 and SCP-079")]
    public bool BetterScp268 = false;
}