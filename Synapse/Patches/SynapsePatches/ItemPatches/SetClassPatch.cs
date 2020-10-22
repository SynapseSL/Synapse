using System.Collections.Generic;
using Harmony;
using Mirror;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetPlayersClass))]
    internal static class SetClassPatch
    {
        private static void Prefix(CharacterClassManager __instance, ref List<SynapseItem> __state, GameObject ply, ref bool escape)
        {
            if (!NetworkServer.active) return;
            var player = ply.GetPlayer();
            if (player.Hub.isDedicatedServer || !player.Hub.Ready) return;

            if(escape && CharacterClassManager.KeepItemsAfterEscaping)
            {
                __state = new List<SynapseItem>();
                foreach(var item in player.Inventory.Items)
                {
                    item.Despawn();
                    __state.Add(item);
                }
                escape = false;
                return;
            }
            __state = null;
            //WHY THE FUCK DOES SCP DONT USE THEY OWN METHODS TO CLEAR THE INVENTORY THAT I ALREADY PATCHED?
            player.Inventory.Clear();
        }

        private static void Postfix(CharacterClassManager __instance, List<SynapseItem> __state, GameObject ply)
        {
            if (__state == null) return;
            var player = ply.GetPlayer();
            
            foreach(var item in __state)
            {
                if (CharacterClassManager.PutItemsInInvAfterEscaping)
                {
                    var itemByID = player.VanillaInventory.GetItemByID(item.ItemType);
                    var flag = false;
                    var categories = __instance._search.categories;
                    int i = 0;
                    while (i < categories.Length)
                    {
                        var invcategorie = categories[i];
                        if (invcategorie.itemType == itemByID.itemCategory && itemByID.itemCategory != ItemCategory.None)
                        {
                            int num = 0;
                            foreach (var sync in player.VanillaInventory.items)
                                if (player.VanillaInventory.GetItemByID(sync.id).itemCategory == itemByID.itemCategory)
                                    num++;

                            if (num >= (int)invcategorie.maxItems)
                            {
                                flag = true;
                                break;
                            }
                            break;
                        }
                        else
                            i++;
                    }

                    if (player.VanillaInventory.items.Count >= 8 || (flag && !item.IsCustomItem))
                        item.Drop(__instance._pms.RealModelPosition);
                    else
                        item.PickUp(player);
                }
                else
                    item.Drop(__instance._pms.RealModelPosition);
            }
        }
    }
}
