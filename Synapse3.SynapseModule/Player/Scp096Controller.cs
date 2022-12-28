using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Discord;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp096;
using PluginAPI.Core;
using Synapse3.SynapseModule.Map.Objects;
using YamlDotNet.Core.Tokens;

namespace Synapse3.SynapseModule.Player;

public class Scp096Controller
{
    
    private readonly SynapsePlayer _player;
    
    internal Scp096Controller(SynapsePlayer player)
    {
        _player = player;
    }
    
    
    public Scp096Role Role => _player.CurrentRole as Scp096Role;
    public Scp096RageManager RageManager => Role?.GetSubroutine<Scp096RageManager>();
    public Scp096ChargeAbility ChargeAbility => Role?.GetSubroutine<Scp096ChargeAbility>();
    public DynamicHumeShieldController Shield => Role.HumeShieldModule as DynamicHumeShieldController;

    public bool Is096Instance => Role != null;

    public float ShieldAmount
    {
        get
        {
            if (Is096Instance) return Shield.HsCurrent;
            return 0;
        }
        set
        {
            if (!Is096Instance) return;
            Shield.HsCurrent = value;
        }
    }

    private float _maxShield = -1;

    /// <summary>
    /// The max shiled of 096 when he start is rage (set to -1 to use the default one)
    /// </summary>
    public float MaxShield
    {
        get
        {
            if (Is096Instance)
            {
                if (_maxShield == -1)
                    return DefaultMaxShield;
                else
                    return _maxShield;
            }
            return 0f;
        }
        set
        {
            _maxShield = value;
        }
    }

    public float DefaultMaxShield
    {
        get
        {
            if (Is096Instance) 
                return Shield.ShieldOverHealth.Evaluate(Shield._hp.NormalizedValue);
            return 0f;
        }
    }

    /// <summary>
    /// The regeneration shiled of 096 when he start is rage (set to -1 to use the default one)
    /// </summary>
    public float ShieldRegeneration
    {
        get
        {
            if (Is096Instance) return Shield.RegenerationRate;
            return 0f;
        }
        set
        {
            if (!Is096Instance) return;
            Shield.RegenerationRate = value;
        }
    }

    public float EnrageTimeLeft
    {
        get
        {
            if (Is096Instance) return RageManager.EnragedTimeLeft;
            return 0f;
        }
        set
        {
            if (!Is096Instance) return;
            RageManager.EnragedTimeLeft = value;
        }
    }

    public Scp096RageState RageState
    {
        get
        {
            if (Is096Instance) return Role.StateController.RageState;
            return Scp096RageState.Docile;
        }
        set
        {
            if (!Is096Instance) return;
            Role.StateController.RageState = value;
        }
    }

    public ReadOnlyCollection<SynapsePlayer> Targets
    {
        get
        {
            if (!Is096Instance) return new ReadOnlyCollection<SynapsePlayer>(new List<SynapsePlayer>());
            return new ReadOnlyCollection<SynapsePlayer>(RageManager
                ._targetsTracker.Targets.Select(x => x.GetSynapsePlayer()).ToList());
        }
    }

    public bool CanAttack
    {
        get
        {
            if (Is096Instance) return RageManager.IsEnraged;
            return false;
        }
    }

    public bool CanCharge
    {
        get
        {
            if (Is096Instance) return ChargeAbility.CanCharge;
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
        if (Is096Instance) return;

        Scp096.ChargeDoor(door.Variant);
    }
    */
}