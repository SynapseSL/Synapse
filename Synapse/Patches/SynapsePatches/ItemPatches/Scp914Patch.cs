using System;
using System.Collections.Generic;
using Harmony;
using Scp914;
using Synapse.Api;
using Synapse.Api.Items;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Scp914Machine),nameof(Scp914Machine.UpgradeItem))]
    internal static class Scp914Patch
    {
        private static bool Prefix(Scp914Machine __instance,ref bool __result,Pickup item)
        {
            try
            {
                var type = __instance.UpgradeItemID(item.itemId);
                var synapseitem = item.GetSynapseItem();

                if (synapseitem.IsCustomItem) return false;

                if (type < ItemType.KeycardJanitor)
                {
                    synapseitem.Destroy();
                    __result = false;
                    return false;
                }
                synapseitem.pickup = null;
                synapseitem.Destroy();

                var newitem = new SynapseItem(type, item.durability, item.weaponMods[0], item.weaponMods[1], item.weaponMods[2]);
                newitem.pickup = item;
                newitem.Position = newitem.pickup.transform.position;
                newitem.pickup.RefreshDurability();
                if(newitem.ItemType == ItemType.GunLogicer)
                {
                    newitem.Sight = 0;
                    newitem.Barrel = 0;
                    newitem.Other = 0;
                }
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp914ItemUpgrade failed!!\n{e}");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradePlayer))]
    internal static class Scp914Patch2
    {
        private static bool Prefix(Scp914Machine __instance, Inventory inventory, CharacterClassManager player, IEnumerable<CharacterClassManager> players)
        {
            try
            {
                var splayer = inventory.GetPlayer();
                foreach(var item in splayer.Inventory.Items)
                {
                    if (item.IsCustomItem) continue;

                    var type = __instance.UpgradeItemID(item.ItemType);

                    if (type < ItemType.KeycardJanitor)
                        item.Destroy();
                    else
                    {
                        var newitem = new SynapseItem(type, item.Durabillity, item.Sight, item.Barrel, item.Other);
                        item.Destroy();
                        if(newitem.ItemType == ItemType.GunLogicer)
                        {
                            newitem.Sight = 0;
                            newitem.Barrel = 0;
                            newitem.Other = 0;
                        }
                        newitem.PickUp(splayer);
                        Scp914Machine.TryFriendshipAchievement(type, player, players);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp914PlayerUpgrade failed!!\n{e}");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.UpgradeHeldItem))]
    internal static class Scp914Patch3
    {
        private static bool Prefix(Scp914Machine __instance, Inventory inventory, CharacterClassManager player, IEnumerable<CharacterClassManager> players)
        {
            try
            {
                if (inventory.curItem == ItemType.None) return false;

                var splayer = inventory.GetPlayer();
                var type = __instance.UpgradeItemID(splayer.ItemInHand.ItemType);
                var index = inventory.GetItemIndex();
                if (index < 0 || index >= inventory.items.Count) return false;
                if (splayer.ItemInHand.IsCustomItem) return false;
                if (type == ItemType.None)
                {
                    splayer.ItemInHand.Destroy();
                    return false;
                }

                var item = splayer.ItemInHand;
                var newitem = new SynapseItem(type, item.Durabillity, item.Sight, item.Barrel, item.Other);
                if(type == ItemType.GunLogicer)
                {
                    newitem.Barrel = 0;
                    newitem.Sight = 0;
                    newitem.Other = 0;
                }
                item.Destroy();
                newitem.PickUp(splayer);

                Scp914Machine.TryFriendshipAchievement(type, player, players);
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: Scp914HeldItemUpgrade failed!!\n{e}");
            }
            return false;
        }
    }
}
