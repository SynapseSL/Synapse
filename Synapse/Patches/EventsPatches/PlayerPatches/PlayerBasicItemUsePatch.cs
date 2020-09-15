using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GameCore;
using Harmony;
using Synapse.Api.Events;
using Synapse.Api.Events.SynapseEventArguments;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.UseMedicalItem))]
    public class PlayerBasicItemUsePatch
    {
        
        //TODO: Clear on Round restart
        internal static Dictionary<int,ItemType> HealCache = new Dictionary<int, ItemType>();
        
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                SynapseController.Server.Logger.Info($"ItemUse: { __instance._hub.inventory.curItem.ToString()}");

                var t = __instance._hub.inventory.curItem;

                HealCache.Add(__instance.GetPlayer().PlayerId, t);
                
                var allow = true;
                SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(__instance.GetPlayer(), __instance._hub.inventory.curItem, ItemUseState.Initiating, ref allow);
                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: BasicItemUse failed!!\n{e}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdCancelMedicalItem))]
    public class CancelBasicItemUse
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void Postfix(ConsumableAndWearableItems __instance)
        {
            PlayerBasicItemUsePatch.HealCache.Remove(__instance.GetPlayer().PlayerId);
        }
    }
}