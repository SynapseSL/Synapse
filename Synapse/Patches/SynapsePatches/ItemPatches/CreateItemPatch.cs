using System;
using HarmonyLib;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Inventory),nameof(Inventory.AddNewItem))]
    internal static class AddNewItemPatch
    {
        private static bool Prefix(Inventory __instance, ItemType id, float dur = -4.65664672E+11f, int s = 0, int b = 0, int o = 0)
        {
            var player = __instance.GetPlayer();
            if (player.VanillaItems.Count >= 8) return false;

            var vanillaitem = new Item(__instance.GetItemByID(id));

            if(!(Math.Abs(dur - -4.65664672E+11f) > 0.05f))
            {
                dur = vanillaitem.durability;

                for (int i = 0; i < __instance._weaponManager.weapons.Length; i++)
                {
                    if(__instance._weaponManager.weapons[i].inventoryID == id)
                    {
                        s = __instance._weaponManager.modPreferences[i, 0];
                        b = __instance._weaponManager.modPreferences[i, 1];
                        o = __instance._weaponManager.modPreferences[i, 2];
                    }
                }
            }

            var item = new Synapse.Api.Items.SynapseItem(id, dur, s, b, o);
            item.PickUp(player);
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory),nameof(Inventory.SetPickup))]
    internal static class SetPickupPatch
    {
        private static void Postfix(Pickup __result)
        {
            try
            {
                var item = new Synapse.Api.Items.SynapseItem(__result.itemId, __result.durability, __result.weaponMods[0], __result.weaponMods[1], __result.weaponMods[2]);
                item.pickup = __result;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Item: Create PickUp failed!!\n{e}");
            }
        }
    }
}
