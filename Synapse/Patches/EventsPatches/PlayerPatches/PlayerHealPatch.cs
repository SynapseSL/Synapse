using System;
using Harmony;
using Synapse.Api.Events.SynapseEventArguments;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HealHPAmount))]
    public class PlayerHealPatch
    {
        private static bool Prefix(PlayerStats __instance, ref float hp)
        {
            try
            {
                var player = __instance.GetPlayer();
                var allow = true;
                var doNotCallHeal = false;
                if (PlayerBasicItemUsePatch.HealCache.ContainsKey(player.PlayerId))
                {
                    var item = PlayerBasicItemUsePatch.HealCache[player.PlayerId];
                    PlayerBasicItemUsePatch.HealCache.Remove(player.PlayerId);
                    
                    var ignoredAllow = true;
                    SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(player, item, ItemInteractState.Finalizing, ref ignoredAllow);

                    if (item == ItemType.SCP268)
                    {
                        doNotCallHeal = true;
                    }
                }

                if (!doNotCallHeal)
                {
                    SynapseController.Server.Events.Player.InvokePlayerHealEvent(player, ref hp, ref allow);
                }
                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerHeal failed!!\n{e}");
            }

            return true;
        }
    }
}