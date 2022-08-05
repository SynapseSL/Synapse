using System;
using System.Collections.Generic;
using System.ComponentModel;
using Syml;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Configuration Section for all GamePlay related stuff
/// </summary>
[Serializable]
[DocumentSection("GamePlay")]
public class GamePlayConfiguration : IDocumentSection
{
    [Description("If Enabled everyone can attack everyone after the Round ended")]
    public bool AutoFriendlyFire { get; set; } = true;

    [Description("If Enabled SCP-268 will hide you even for SCP-096 and SCP-079")]
    public bool BetterScp268{ get; set; } = false;

    [Description("If enabled a Player don't need to equip his keycard to use it")]
    public bool RemoteKeyCard{ get; set; } = false;

    [Description("The amount of persons that need to be sacrificed for SCP-106's recontainment")]
    public ushort RequiredForFemur { get; set; } = 1;
    
    [Description("All Scp's in this list are able to Speak to Humans")]
    public List<uint> SpeakingScp { get; set; } = new List<uint> { 16, 17 };
}