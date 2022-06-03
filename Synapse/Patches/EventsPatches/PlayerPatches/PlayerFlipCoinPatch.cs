using HarmonyLib;
using InventorySystem.Items.Coin;
using Mirror;
using System;
using Utils.Networking;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CoinNetworkHandler), nameof(CoinNetworkHandler.ServerProcessMessage))]
    internal class PlayerFlipCoinPatch
    {
        [HarmonyPrefix]
        private static bool FlipCoinPatch(NetworkConnection conn)
        {
            try
            {
                if (ReferenceHub.TryGetHub(conn.identity.gameObject, out var hub) && hub.inventory.CurItem.TypeId == ItemType.Coin)
                {
                    var isTails = UnityEngine.Random.value >= 0.5f;

                    Server.Get.Events.Player.InvokeFlipCoinEvent(hub.GetPlayer(), ref isTails, out var allow);

                    if (allow)
                        new CoinNetworkHandler.CoinFlipMessage(hub.inventory.CurItem.SerialNumber, isTails).SendToAuthenticated();
                }

                return false;
            }
            catch (Exception ex)
            {
                Synapse.Api.Logger.Get.Error("Synapse-Event: FlipCoinEvent Start failed!!\n" + ex);
                return true;
            }
        }
    }
}