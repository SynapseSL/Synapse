using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MEC;
using Mirror;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdUseMedicalItem))]
    internal static class PlayerBasicItemUsePatch
    {
        private static bool Prefix(ConsumableAndWearableItems __instance)
        {
            if (!__instance._interactRateLimit.CanExecute(true)) return false;

            __instance._cancel = false;
            if (__instance.cooldown > 0f) return false;

            for (int i = 0; i < __instance.usableItems.Length; i++)
                if (__instance.usableItems[i].inventoryID == __instance._hub.inventory.curItem && __instance.usableCooldowns[i] <= 0f)
                {
                    try
                    {
                        var player = __instance.GetPlayer();
                        var item = player.ItemInHand;
                        var allow = true;
                        Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Initiating, ref allow);
                        if (!allow) return false;
                    }
                    catch(Exception e)
                    {
                        Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Initiating failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                    }

                    Timing.RunCoroutine(UseMedialItem(__instance, i));
                }

            return false;
        }

        public static IEnumerator<float> UseMedialItem(ConsumableAndWearableItems consumable,int mid)
        {
            //The only line you can decompile
            if (!NetworkServer.active)
                yield break;

            //Geting all variables
            var player = consumable.GetPlayer();
            var item = player.ItemInHand;
            var usableitem = consumable.usableItems[mid];

            //Start Animation
            consumable.healInProgress = true;
            consumable.SendRpc(ConsumableAndWearableItems.HealAnimation.StartHealing, mid);

            //Wait for Animation
            var start = UnityEngine.Time.time;
            while (UnityEngine.Time.time <= start + usableitem.animationDuration)
            {
                yield return Timing.WaitForOneFrame;
                if(consumable._cancel && UnityEngine.Time.time < start + usableitem.cancelableTime)
                {
                    consumable.SendRpc(ConsumableAndWearableItems.HealAnimation.CancelHealing, mid);
                    yield break;
                }
            }

            //End Animation
            consumable.SendRpc(ConsumableAndWearableItems.HealAnimation.DequipMedicalItem, mid);

            //Event
            try
            {
                var allow = true;
                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Finalizing, ref allow);
                if (!allow) yield break;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent finalizing failed!!\n{e}");
            }

            //Set Cooldown
            consumable.usableCooldowns[mid] = usableitem.cooldownAfterUse;
            consumable.RpcSetCooldown(mid, usableitem.cooldownAfterUse);

            //Adding Health/ArtificialHealth
            if (usableitem.instantHealth > 0)
                player.PlayerStats.HealHPAmount(usableitem.instantHealth);
            if (usableitem.artificialHealth > 0)
                player.ArtificialHealth += usableitem.artificialHealth;


            //Give Effects
            foreach (var effectstring in usableitem.effectsToInitialize)
            {
                var split = effectstring.Split('=');
                if (split.Count() > 1)
                    player.PlayerEffectsController.EnableByString(split[0], float.Parse(split[1]));
                else
                    player.PlayerEffectsController.EnableByString(effectstring);
            }

            //Scp500
            if (usableitem.inventoryID == ItemType.SCP500)
                player.PlayerEffectsController.UseMedicalItem(usableitem.inventoryID);
            
            consumable.healInProgress = false;

            //Destroy Item if it is one time use
            if (usableitem.inventoryID != ItemType.SCP268)
                item.Destroy();

            //HealOver Time
            if (usableitem.regenOverTime != null)
            {
                var process = new ConsumableAndWearableItems.HealthRegenerationProcess(usableitem.regenOverTime);
                var curve = usableitem.regenOverTime;

                //TODO: fix this so that it actualy use the animation curve from above

                float HptoHeal;
                float time;
                switch (usableitem.inventoryID)
                {
                    case ItemType.SCP500:
                        HptoHeal = 38;
                        time = 12;
                        break;

                    case ItemType.Adrenaline:
                        HptoHeal = 49;
                        time = 20;
                        break;

                    case ItemType.Painkillers:
                        HptoHeal = 26;
                        time = 26;
                        break;

                    default: yield break;
                }

                for (float i = 0; i <= time; i += time / HptoHeal) 
                {
                    yield return Timing.WaitForSeconds(time / HptoHeal);
                    consumable.hpToHeal++;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdCancelMedicalItem))]
    public class CancelBasicItemUse
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                var allow = true;
                Server.Get.Events.Player.InvokePlayerItemUseEvent(__instance.GetPlayer(),__instance.GetPlayer().ItemInHand,ItemInteractState.Stopping,ref allow);
                return allow;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerItemUseEvent Stopping failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}