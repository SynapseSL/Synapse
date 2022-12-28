using System;
using System.Collections.Generic;
using System.Reflection;
using CustomPlayerEffects;
using Footprinting;
using Mirror;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using Synapse3.SynapseModule.Patching.Patches;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public class Scp106Controller
{
    //TODO: Add the new SCP 106 Ablility
    
    private readonly SynapsePlayer _player;
    
    internal Scp106Controller(SynapsePlayer player)
    {
        _player = player;
    }

    public Scp106Role Role => _player.CurrentRole as Scp106Role;
    public Scp106StalkAbility StalkAbility => Role?.GetSubroutine<Scp106StalkAbility>();
    public Scp106HuntersAtlasAbility HuntersAtlas => Role?.GetSubroutine<Scp106HuntersAtlasAbility>();
    public Scp106Attack Attack => Role?.GetSubroutine<Scp106Attack>();

    public bool Is106Instance => Role != null;

    public bool IsUsingPortal => Role.Sinkhole.IsDuringAnimation;
    public bool IsInGround => Role.IsSubmerged;

    public void CapturePlayer(SynapsePlayer player)
    {
        NeuronLogger.For<Synapse>().Warn("Is106Instance: ");

        if (Attack != null)
        {
            NeuronLogger.For<Synapse>().Warn("TryGetComponent: ");

            Attack.SendCooldown(Attack._hitCooldown);
            Attack.Vigor.VigorAmount += 0.3f;
            Attack.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarker(Attack.Owner, 1f);
            CallOnPlayerTeleported(player.Hub);
            var effectsController = player.Hub.playerEffectsController;
            effectsController.EnableEffect<Traumatized>(180f);
            effectsController.EnableEffect<Corroding>();
            NeuronLogger.For<Synapse>().Warn("TryGetComponent End: ");
        }
    }

    private void CallOnPlayerTeleported(ReferenceHub hub)
    {
        if (!Is106Instance) return;

        var eventDelegate = (MulticastDelegate)typeof(Scp106AttackPatch)
            .GetField(nameof(Scp106Attack.OnPlayerTeleported), BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(null);
        if (eventDelegate != null)
        {
            foreach (var handler in eventDelegate.GetInvocationList())
            {
                handler.Method.Invoke(handler.Target, new object[] { null, hub });
            }
        }
    }

    public HashSet<SynapsePlayer> PlayersInPocket { get; } = new();

}