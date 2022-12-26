using CustomPlayerEffects;
using HarmonyLib;
using Hazards;
using InventorySystem.Items.MicroHID;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using PlayerStatsSystem;
using PluginAPI.Enums;
using PluginAPI.Events;
using Synapse3.SynapseModule.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("Scp106Attack", PatchType.PlayerEvent)]
public static class Scp106AttackPatch
{
    static ScpEvents _scp;

    static Scp106AttackPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp106Attack), nameof(Scp106Attack.ServerShoot))]
    public static bool OnServerShoot(Scp106Attack __instance)
    {
        try
        {
            Scp106AttackEvent ev;

            using (new FpcBacktracker(__instance._targetHub, __instance._targetPosition, 0.35f))
            {
                Vector3 vector = __instance._targetPosition - __instance._ownerPosition;
                float sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude > __instance._maxRangeSqr)
                {
                    return false;
                }

                Vector3 forward = __instance.OwnerCam.forward;
                forward.y = 0f;
                vector.y = 0f;
                if (Physics.Linecast(__instance._ownerPosition, __instance._targetPosition, MicroHIDItem.WallMask))
                {
                    return false;
                }

                if (__instance._dotOverDistance.Evaluate(sqrMagnitude) > Vector3.Dot(vector.normalized, forward.normalized))
                {
                    __instance.SendCooldown(__instance._missCooldown);
                    return false;
                }

                var player = __instance.Owner.GetSynapsePlayer();
                var victime =__instance._targetHub.GetSynapsePlayer();

                ev = new Scp106AttackEvent(player, victime, __instance._damage, true, true);
                _scp.Scp106Attack.RaiseSafely(ev);

                if (!ev.Allow) return false;

                if (!EventManager.ExecuteEvent(ServerEventType.Scp106TeleportPlayer, __instance.Owner, __instance._targetHub))
                {
                    return false;
                }

                DamageHandlerBase handler = new ScpDamageHandler(__instance.Owner, __instance._damage, DeathTranslations.PocketDecay);
                if (!__instance._targetHub.playerStats.DealDamage(handler))
                {
                    return false;
                }

                player.ScpController.Scp106.PlayersInPocket.Add(victime);
            }

            __instance.SendCooldown(__instance._hitCooldown);
            __instance.Vigor.VigorAmount += 0.3f;
            __instance.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            CallOnPlayerTeleported(__instance._targetHub);
            PlayerEffectsController playerEffectsController = __instance._targetHub.playerEffectsController;
            playerEffectsController.EnableEffect<Traumatized>(180f);
            if (ev.TakeToPocket)
                playerEffectsController.EnableEffect<Corroding>();

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerTrantumHazard failed\n" + ex);
            return true;
        }
    }

    private static void CallOnPlayerTeleported(ReferenceHub hub)
    {
        var field = typeof(Scp106AttackPatch).GetField(nameof(Scp106Attack.OnPlayerTeleported), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        var eventHandlerList = field.GetValue(null);
        var event_invoke = eventHandlerList.GetType().GetMethod("Invoke");
        event_invoke.Invoke(eventHandlerList, new object[1] { hub });
    }
}
