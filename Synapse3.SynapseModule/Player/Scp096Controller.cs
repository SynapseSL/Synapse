using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;
using Mirror;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp096;
using PluginAPI.Core;
using Synapse3.SynapseModule.Map.Objects;
using YamlDotNet.Core.Tokens;

namespace Synapse3.SynapseModule.Player;


public class Scp096Controller : ScpShieldControler<Scp096Role>
{
    public Scp096Controller(SynapsePlayer player) : base(player) { }

    public Scp096RageManager RageManager => Role?.GetSubroutine<Scp096RageManager>();
    public Scp096ChargeAbility ChargeAbility => Role?.GetSubroutine<Scp096ChargeAbility>();
    public override HumeShieldModuleBase SheildModule => Role?.HumeShieldModule;

    public float EnrageTimeLeft
    {
        get
        {
            if (IsInstance) return RageManager.EnragedTimeLeft;
            return 0f;
        }
        set
        {
            if (!IsInstance) return;
            RageManager.EnragedTimeLeft = value;
            RageManager.ServerSendRpc(toAll: true);
        }
    }

    public Scp096RageState RageState
    {
        get
        {
            if (IsInstance) return Role.StateController.RageState;
            return Scp096RageState.Docile;
        }
        set
        {
            if (!IsInstance) return;
            Role.StateController.RageState = value;
        }
    }

    public ReadOnlyCollection<SynapsePlayer> Targets
    {
        get
        {
            if (!IsInstance) return new ReadOnlyCollection<SynapsePlayer>(new List<SynapsePlayer>());
            return new ReadOnlyCollection<SynapsePlayer>(RageManager
                ._targetsTracker.Targets.Select(x => x.GetSynapsePlayer()).ToList());
        }
    }

    public bool CanAttack
    {
        get
        {
            if (IsInstance) return RageManager.IsEnraged;
            return false;
        }
    }

    public bool CanCharge
    {
        get
        {
            if (IsInstance) return ChargeAbility.CanCharge;
            return false;
        }
    }

    public void AddTarget(SynapsePlayer player)
    {
        var rageManager = RageManager;
        if (rageManager != null) return;

        rageManager._targetsTracker.AddTarget(player);
    }

    public void RemoveTarget(SynapsePlayer player)
    {
        var rageManager = RageManager;
        if (rageManager != null) return;

        RageManager._targetsTracker.RemoveTarget(player);
    }
    /*TODO:
    public void ChargeDoor(SynapseDoor door)
    {
        if (IsInstance) return;

        Scp096.ChargeDoor(door.Variant);
    }
    */
}