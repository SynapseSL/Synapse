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
        private static bool FlipCoinPatch(
          NetworkConnection conn,
          CoinNetworkHandler.CoinFlipMessage msg)
        {
            try
            {
                ReferenceHub hub;
                if (ReferenceHub.TryGetHub(conn.identity.gameObject, out hub) && hub.inventory.CurItem.TypeId == ItemType.Coin)
                {
                    var isTails = (double)UnityEngine.Random.value >= 0.5;
                    
                    Server.Get.Events.Player.InvokeFlipCoinEvent(hub.GetPlayer(), ref isTails, out var allow);
                    
                    if (allow)
                        new CoinNetworkHandler.CoinFlipMessage(hub.inventory.CurItem.SerialNumber, isTails).SendToAuthenticated<CoinNetworkHandler.CoinFlipMessage>();
                }
                return false;
            }
            catch (Exception ex)
            {
                Synapse.Api.Logger.Get.Error(string.Format("Synapse-Event: FlipCoinEvent Start failed!!\n{0}", (object)ex));
                return true;
            }
        }
    }
}