using System;
using System.Collections.Generic;
using Achievements;
using HarmonyLib;
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
    private static bool OnGetAllHubs(out Dictionary<GameObject, ReferenceHub> __result)
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
    private static bool OnTargetRpc(NetworkBehaviour __instance)
    {
        var player = __instance.GetPlayer();
        if (player.PlayerType == PlayerType.Dummy) return false;
        return true;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AchievementHandlerBase), nameof(AchievementHandlerBase.ServerAchieve))]
    private static bool OnAchieve(NetworkConnection conn) => conn != null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.ShowHitIndicator))]
    private static bool OnIndicator(uint netId)
    {
        return Synapse.Get<PlayerService>().GetPlayer(netId).Connection != null;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    private static Exception OnShoot(Exception __exception)
    {
        NeuronLogger.For<Synapse>().Error(__exception);
        return null;
    }
}