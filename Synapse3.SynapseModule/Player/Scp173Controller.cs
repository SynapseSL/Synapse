 using Hazards;
using Mirror;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp173;
using PluginAPI.Core;
using RelativePositioning;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class Scp173Controller : ScpShieldControler<Scp173Role> 
{
    
    public Scp173Controller(SynapsePlayer player) : base(player) { }
    
    public Scp173ObserversTracker ObserverTraker => Role?.GetSubroutine<Scp173ObserversTracker>();
    public Scp173BlinkTimer BlinkTimer => Role?.GetSubroutine<Scp173BlinkTimer>();
    public Scp173AudioPlayer AudioPlayer => Role?.GetSubroutine<Scp173AudioPlayer>();
    public Scp173BreakneckSpeedsAbility BreakneckSpeedsAbility => Role?.GetSubroutine<Scp173BreakneckSpeedsAbility>();
    public Scp173SnapAbility SnapAbility => Role?.GetSubroutine<Scp173SnapAbility>();
    public Scp173TantrumAbility TantrumAbility => Role?.GetSubroutine<Scp173TantrumAbility>();
    public Scp173TeleportAbility TeleportAbility => Role?.GetSubroutine<Scp173TeleportAbility>();
    public override HumeShieldModuleBase SheildModule => Role?.HumeShieldModule;


    public bool Is173Instance => Role != null;

    public bool IsObserved => ObserverTraker?.IsObserved ?? false;

    //TODO: Patch
    private HashSet<SynapsePlayer> _observer = new HashSet<SynapsePlayer>();
    public HashSet<SynapsePlayer> Observer => _observer;

    public float CurentTantrumCoolDown
    {
        get
        {
            var ability = TantrumAbility;
            if (ability == null) return 0;

            return ability.Cooldown.Remaining;
        }
        set
        {
            var ability = TantrumAbility;
            if (ability == null) return;

            ability.Cooldown.Trigger(value);
            ability.ServerSendRpc(toAll: true);
        }
    }

    public float TantrumCoolDown { get; set; } = Scp173TantrumAbility.CooldownTime;


    public float CurentBlinkCooldown
    {
        get => BlinkTimer?.RemainingBlinkCooldown ?? 0f;
        set
        {
            var blinkTimer = BlinkTimer;
            if (blinkTimer == null)
                return;
            blinkTimer._initialStopTime = value + NetworkTime.time;
            blinkTimer.ServerSendRpc(toAll: true);
        }
    }

    public float BlinkCooldownBase { get; set; } = Scp173BlinkTimer.CooldownBaseline;

    public float BlinkCooldownPerPlayer { get; set; } = Scp173BlinkTimer.CooldownPerObserver;

    public float SpeedBoostCoolDown
    {
        get
        {
            var ability = BreakneckSpeedsAbility;
            if (ability == null) return 0;

            return ability.Cooldown.Remaining;
        }
        set
        {
            var ability = BreakneckSpeedsAbility;
            if (ability == null) return;

            ability.Cooldown.Remaining = value;
            ability.ServerSendRpc(toAll: true);
        }
    }

    public bool Speeding
    {
        get
        {
            var ability = BreakneckSpeedsAbility;
            if (ability == null) return false;

            return ability.IsActive;
        }
        set
        {
            var ability = BreakneckSpeedsAbility;
            if (ability == null) return;

            BreakneckSpeedsAbility.IsActive = value;
            ability.ServerSendRpc(toAll: true);
        }
    }

    public void CreateTantrum(Vector3 postion, float coolDown = 30)
    {
        CurentTantrumCoolDown = coolDown;
        var tantrum = Object.Instantiate(TantrumAbility._tantrumPrefab);
        tantrum.SynchronizedPosition = new RelativePosition(postion);
        NetworkServer.Spawn(tantrum.gameObject);
    }

    internal void ResetDefault()
    {
        BlinkCooldownBase = Scp173BlinkTimer.CooldownBaseline;
        BlinkCooldownPerPlayer = Scp173BlinkTimer.CooldownPerObserver;
        Observer.Clear();
    }
}