using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.BasicMessages;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
internal static class ShootPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    private static bool OnShootMsg(NetworkConnection conn, ShotMessage msg)
    {
        try
        {
            var ev = new ShootEvent(conn.GetPlayer(), msg.TargetNetId, msg.ShooterWeaponSerial, true);
            ev.Raise();
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Player Shoot Event failed\n" + ex);
            return true;
        }
    }
}