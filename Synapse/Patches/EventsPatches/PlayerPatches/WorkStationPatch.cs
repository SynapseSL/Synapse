using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.Attachments;
using Event = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(WorkstationController),nameof(WorkstationController.ServerInteract))]
    internal static class WorkStationPatch
    {
        [HarmonyPrefix]
        private static bool Interact(WorkstationController __instance, ReferenceHub ply, byte colliderId)
        {
            try
            {
                if (colliderId != __instance._activateCollder?.ColliderId || __instance.Status != 0) return false;

                if (ply is null) return false;

                var player = ply.GetPlayer();
                var station = __instance.GetWorkStation();

                if (station != null)
                    station.KnownUser = player;

                Event.Get.Player.InvokePlayerStartWorkstation(player, station, out var allow);

                return allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: Connect Workstation Tablet failed!!\n{e}");
                return false;
            }
        }
    }
}
