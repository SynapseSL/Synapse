using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CustomPlayerEffects;
using Footprinting;
using Mirror;
using Neuron.Core.Logging;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using Synapse3.SynapseModule.Patching.Patches;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;


public class Scp106Controller : ScpShieldControler<Scp106Role>
{
    //TODO: Add the new SCP 106 Ablility

    public Scp106Controller(SynapsePlayer player) : base(player) { }

    public Scp106StalkAbility StalkAbility => Role?.GetSubroutine<Scp106StalkAbility>();
    public Scp106HuntersAtlasAbility HuntersAtlas => Role?.GetSubroutine<Scp106HuntersAtlasAbility>();
    public Scp106Attack Attack => Role?.GetSubroutine<Scp106Attack>();
    public override HumeShieldModuleBase SheildModule => Role?.HumeShieldModule;

    public bool IsUsingPortal => Role.Sinkhole.IsDuringAnimation;
    public bool IsInGround => Role.IsSubmerged;

    public void CapturePlayer(SynapsePlayer player)
    {
        if (Attack != null)
        {
            Attack.SendCooldown(Attack._hitCooldown);
            Attack.Vigor.VigorAmount += 0.3f;
            Attack.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarker(Attack.Owner, 1f);
            Synapse3Extensions.RaiseEvent(typeof(Scp106Attack), nameof(Scp106Attack.OnPlayerTeleported), player.Hub);
            var effectsController = player.Hub.playerEffectsController;
            effectsController.EnableEffect<Traumatized>(180f);
            effectsController.EnableEffect<Corroding>();
            NeuronLogger.For<Synapse>().Warn("TryGetComponent End: ");
        }
    }

    public HashSet<SynapsePlayer> PlayersInPocket { get; } = new();

}