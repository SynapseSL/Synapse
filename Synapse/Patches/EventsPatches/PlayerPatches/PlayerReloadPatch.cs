﻿using System;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using Mirror;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    internal class PlayerReloadPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerRequestReceived))]
        private static bool Prefix(NetworkConnection conn, RequestMessage msg)
        {
            try
            {
                if (msg.Request != RequestType.Reload) return true;

                var player = conn.GetPlayer();
                var item = player.ItemInHand;
                var allow = true;

                if (player == null || item == null) return false;
                if (item.Serial != msg.Serial) return false;
                if (!(item.ItemBase is Firearm)) return false;


                SynapseController.Server.Events.Player.InvokePlayerReloadEvent(player, ref allow, item);

                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerReload failed!!\n{e}");
                return true;
            }
        }
    }
}