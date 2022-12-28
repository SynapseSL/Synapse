using Hazards;
using Mirror;
using PlayerRoles.PlayableScps.Scp173;
using RelativePositioning;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class Scp173Controller
{
    
    private SynapsePlayer _player;
    
    public Scp173Controller(SynapsePlayer player)
    {
        _player = player;
    }
    
    
    public Scp173Role Role => _player.CurrentRole as Scp173Role;
    //TODO : set to cache the subrootine when player spawn
    public Scp173ObserversTracker ObserverTraker => Role?.GetSubroutine<Scp173ObserversTracker>();
    public Scp173BlinkTimer BlinkTimer => Role?.GetSubroutine<Scp173BlinkTimer>();
    public Scp173AudioPlayer AudioPlayer => Role?.GetSubroutine<Scp173AudioPlayer>();
    public Scp173BreakneckSpeedsAbility BreakneckSpeedsAbility => Role?.GetSubroutine<Scp173BreakneckSpeedsAbility>();
    public Scp173SnapAbility SnapAbility => Role?.GetSubroutine<Scp173SnapAbility>();
    public Scp173TantrumAbility TantrumAbility => Role?.GetSubroutine<Scp173TantrumAbility>();
    public Scp173TeleportAbility TeleportAbility => Role?.GetSubroutine<Scp173TeleportAbility>();

    public bool Is173Instance => Role != null;

    public bool IsObserved => ObserverTraker?.IsObserved ?? false;

    //TODO: Patch
    private HashSet<SynapsePlayer> _observer = new HashSet<SynapsePlayer>();
    public HashSet<SynapsePlayer> Observer => _observer;

    public float CurentTantrumCoolDown
    {
        get => TantrumAbility.Cooldown.Remaining;
        set
        {
            TantrumAbility.ServerSendRpc(toAll: true);
            TantrumAbility.Cooldown.Trigger(value);
        }
    }

/*    public float TantrumCoolDown// TODO: Patch
    {
        get;
        set;
    }*/


    /*    public float CurentBlinkCooldown//TODO: Patch
        {
            get;
            set;

        }*/

    public float BlinkCooldown
    {
        get => BlinkTimer?.TotalCooldownServer ?? 0f;
        set
        {
            var blinkTimer = BlinkTimer;
            if (blinkTimer == null)
                return;
            blinkTimer._totalCooldown = value * (BlinkTimer._breakneckSpeedsAbility.IsActive ? 1f : 2f);
        }
    }

    public float BreakneckCoolDown
    {
        get => BreakneckSpeedsAbility.Cooldown.Remaining;
        set => BreakneckSpeedsAbility.Cooldown.Remaining = value;
    }

    public bool IsSpeeding
    {
        get => BreakneckSpeedsAbility.IsActive;
        set => BreakneckSpeedsAbility.IsActive = value;
    }


    public void CreateTrantrum(Vector3 postion)
    {
        TantrumAbility.Cooldown.Trigger(30f);
        TantrumAbility.ServerSendRpc(toAll: true);
        TantrumEnvironmentalHazard tantrumEnvironmentalHazard = Object.Instantiate(TantrumAbility._tantrumPrefab);
        tantrumEnvironmentalHazard.SynchronizedPosition = new RelativePosition(postion);
        NetworkServer.Spawn(tantrumEnvironmentalHazard.gameObject);
    }
    
}