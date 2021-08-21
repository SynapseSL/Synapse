using System;
using HarmonyLib;
using InventorySystem;
using Synapse.Api.Enum;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.UserCode_CmdDropAmmo))]
    internal static class PlayerDropAmmoPatch
    {
        private static bool Prefix(Inventory __instance, ref byte ammoType, ref ushort amount)
        {
            try
            {
                var player = __instance.GetPlayer();
                var type = (AmmoType)ammoType;

                SynapseController.Server.Events.Player.
                    InvokePlayerDropAmmoEvent(player, ref type, ref amount, out var allow);

                ammoType = (byte)type;
                return allow;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerAmmoDrop failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return false;
            }
        }
    }
}
