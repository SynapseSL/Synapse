using System;
using System.Linq;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.CallCmdSetUnic))]
    internal static class PlayerChangeItemPatch
    {
        private static bool Prefix(Inventory __instance, int i)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || __instance._amnesia.Enabled) return false;

                var player = __instance.GetPlayer();
                var olditem = player.ItemInHand;
                var newitem = player.VanillaInventory.items.FirstOrDefault(x => x.uniq == i).GetSynapseItem();

                Server.Get.Events.Player.InvokeChangeItem(player, olditem, newitem);

                __instance.NetworkitemUniq = i;
                return false;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerChangeItem event failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
