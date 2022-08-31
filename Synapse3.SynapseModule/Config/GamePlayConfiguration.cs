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

    [Description("If Enabled the button inside the AlphaWarhead(outside) can be closed again with a keycard")]
    public bool CloseWarheadButton { get; set; } = false;

    [Description("Every Role in this List won't stop SCP-173 from moving when the player is looking at it")]
    public List<uint> CantObserve173 { get; set; } = new()
    {
        (uint)RoleType.Scp173,
        (uint)RoleType.Scp106,
        (uint)RoleType.Scp049,
        (uint)RoleType.Scp079,
        (uint)RoleType.Scp096,
        (uint)RoleType.Scp0492,
        (uint)RoleType.Scp93953,
        (uint)RoleType.Scp93989,
        (uint)RoleType.Tutorial
    };

    public List<uint> CantObserve096 { get; set; } = new()
    {
        (uint)RoleType.Scp173,
        (uint)RoleType.Scp106,
        (uint)RoleType.Scp049,
        (uint)RoleType.Scp079,
        (uint)RoleType.Scp096,
        (uint)RoleType.Scp0492,
        (uint)RoleType.Scp93953,
        (uint)RoleType.Scp93989,
        (uint)RoleType.Tutorial
    };
}