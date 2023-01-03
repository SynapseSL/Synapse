using System;
using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;
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

    [Description("If enabled the Warhead button can be closed again ")]
    public bool WarheadButtonClosable { get; set; } = false;
    
    [Description("All Scp's in this list are able to Speak to Humans")]
    public List<uint> SpeakingScp { get; set; } = new List<uint> //TODO
    { 
        (uint)RoleTypeId.Scp939 
    };

    [Description("When enabled are Chaos and SCP's forced to kill each other since otherwise the Round won't end")]
    public bool ChaosAndScpEnemy { get; set; } = false;

    [Description("Every Role in this List won't stop SCP-173 from moving when the player is looking at it")]
    public List<uint> CantObserve173 { get; set; } = new()
    {
        (uint)RoleTypeId.Scp173,
        (uint)RoleTypeId.Scp106,
        (uint)RoleTypeId.Scp049,
        (uint)RoleTypeId.Scp079,
        (uint)RoleTypeId.Scp096,
        (uint)RoleTypeId.Scp0492,
        (uint)RoleTypeId.Scp939,
        (uint)RoleTypeId.Tutorial
    };

    public List<uint> CantObserve096 { get; set; } = new()
    {
        (uint)RoleTypeId.Scp173,
        (uint)RoleTypeId.Scp106,
        (uint)RoleTypeId.Scp049,
        (uint)RoleTypeId.Scp079,
        (uint)RoleTypeId.Scp096,
        (uint)RoleTypeId.Scp0492,
        (uint)RoleTypeId.Scp939,
        (uint)RoleTypeId.Tutorial
    };
}