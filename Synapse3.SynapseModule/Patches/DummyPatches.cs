using System;
using System.Collections.Generic;
using Achievements;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class DummyPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.GetAllHubs))]
    public static bool OnGetAllHubs(out Dictionary<GameObject, ReferenceHub> __result)
    {
        __result = new Dictionary<GameObject, ReferenceHub>();
        var service = Synapse.Get<PlayerService>();
        foreach (var player in service.Players)
        {
            __result[player.gameObject] = player;
        }

        __result[service.Host.gameObject] = service.Host;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetworkBehaviour), nameof(NetworkBehaviour.SendTargetRPCInternal))]
    public static bool OnTargetRpc(NetworkBehaviour __instance)
    {
        var player = __instance.GetSynapsePlayer();
        if (player.PlayerType == PlayerType.Dummy) return false;
        return true;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AchievementHandlerBase), nameof(AchievementHandlerBase.ServerAchieve))]
    public static bool OnAchieve(NetworkConnection conn) => conn != null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.ShowHitIndicator))]
    public static bool OnIndicator(uint netId)
    {
        return Synapse.Get<PlayerService>().GetPlayer(netId).Connection != null;
    }
    

    //The Dummy is not properly Spawned - need to investigate this later - could cause other bugs as well
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.NetworkId), MethodType.Getter)]
    public static bool GetNetworkID(HitboxIdentity __instance, out uint __result)
    {
        if (__instance.TargetHub == null || __instance.TargetHub.networkIdentity == null)
        {
            var player = __instance.transform.GetComponentInParent<SynapsePlayer>();
            __instance.TargetHub = player.Hub;
            __result = player.NetworkIdentity.netId;
        }
        else
        {
            __result = __instance.TargetHub.inventory.netId;
        }
        return false;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    public static Exception OnShoot(Exception __exception)
    {
        if (__exception != null)
            NeuronLogger.For<Synapse>().Error("Sy3 API: Dummy Shoot fail check activated:\n" + __exception);
        return null;
    }
}