using System;
using System.Collections.Generic;
using Harmony;
using Mirror;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetPlayersClass))]
    internal static class SetClassPatch
    {
        private static bool Prefix(CharacterClassManager __instance, ref PlayerSetClassEventArgs __state, ref RoleType classid, GameObject ply, ref bool escape)
        {
            if (!NetworkServer.active) return false;
            var player = ply.GetPlayer();
            if (player.Hub.isDedicatedServer || !player.Hub.Ready) return false;

            __state = new PlayerSetClassEventArgs();
            __state.EscapeItems = new List<SynapseItem>();

            if (escape && CharacterClassManager.KeepItemsAfterEscaping)
            {
                foreach (var item in player.Inventory.Items)
                {
                    item.Despawn();
                    __state.EscapeItems.Add(item);
                }
                escape = false;
            }

            __state.Allow = true;
            __state.Player = player;
            __state.Role = classid;
            foreach (var id in __instance.Classes.SafeGet(classid).startItems)
            {
                var synapseitem = new SynapseItem(id, 0, 0, 0, 0);
                var item = new Item(player.VanillaInventory.GetItemByID(id));
                synapseitem.Durabillity = item.durability;

                for (int i = 0; i < player.VanillaInventory._weaponManager.weapons.Length; i++)
                {
                    if (player.VanillaInventory._weaponManager.weapons[i].inventoryID == id)
                    {
                        synapseitem.Sight = player.VanillaInventory._weaponManager.modPreferences[i, 0];
                        synapseitem.Barrel = player.VanillaInventory._weaponManager.modPreferences[i, 1];
                        synapseitem.Other = player.VanillaInventory._weaponManager.modPreferences[i, 2];
                    }
                }

                __state.Items.Add(synapseitem);
            }
            try
            {
                Server.Get.Events.Player.InvokeSetClassEvent(__state);
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerSetClass failed!!\n{e}");
            }
            classid = __state.Role;

            if (!__state.Allow) return false;

            //WHY THE FUCK DOES SCP DONT USE THEY OWN METHODS TO CLEAR THE INVENTORY THAT I ALREADY PATCHED?
            player.Inventory.Clear();

            return true;
        }

        private static void Postfix(CharacterClassManager __instance, PlayerSetClassEventArgs __state, GameObject ply)
        {
            if (__state == null) return;
            if (!__state.Allow) return;

            var player = ply.GetPlayer();

            player.Inventory.Clear();
            foreach (var item in __state.Items)
                player.Inventory.AddItem(item);

            if (__state.EscapeItems.Count == 0) return;
            foreach(var item in __state.EscapeItems)
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
