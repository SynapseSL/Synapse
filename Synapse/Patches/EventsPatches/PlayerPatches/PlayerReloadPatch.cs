using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdReload))]
    internal class PlayerReloadPatch
    {
        private static bool Prefix(WeaponManager __instance, bool animationOnly)
        {
            try
            {

                if (animationOnly) return true;
                
                var allow = true;
                var itemIndex = __instance._hub.inventory.GetItemIndex();
                var item = __instance._hub.inventory.items[itemIndex].GetSynapseItem();
                var player = __instance._hub.GetPlayer();

                SynapseController.Server.Events.Player.InvokePlayerReloadEvent(player, ref allow, item);

                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerReload failed!!\n{e}");
                return true;
            }
        }
    }
}