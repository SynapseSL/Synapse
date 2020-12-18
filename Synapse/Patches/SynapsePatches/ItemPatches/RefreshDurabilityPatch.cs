using System;
using HarmonyLib;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Pickup),nameof(Pickup.RefreshDurability))]
    internal static class RefreshDurabilityPatch
    {
        private static bool Prefix(Pickup __instance, bool allowAmmoRenew = false, bool setupAttachments = false)
        {
            var itembyid = Pickup.Inv.GetItemByID(__instance.itemId);
            var item = __instance.GetSynapseItem();
            if (!itembyid.noEquipable || allowAmmoRenew)
                item.Durabillity = itembyid.durability;

            if (!setupAttachments) return false;

            foreach(var weapon in Pickup.Inv.GetComponent<WeaponManager>().weapons)
                if(weapon.inventoryID == __instance.itemId)
                {
                    try
                    {
                        item.Sight = Mathf.Max(0, UnityEngine.Random.Range(-weapon.mod_sights.Length / 2, weapon.mod_sights.Length));
                        item.Barrel = Mathf.Max(0, UnityEngine.Random.Range(-weapon.mod_barrels.Length / 2, weapon.mod_barrels.Length));
                        item.Other = Mathf.Max(0, UnityEngine.Random.Range(-weapon.mod_others.Length / 2, weapon.mod_others.Length));
                    }
                    catch(Exception e)
                    {
                        Logger.Get.Error($"Synapse-Event: RefreshDurability failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                    }
                }
            return false;
        }
    }
}
