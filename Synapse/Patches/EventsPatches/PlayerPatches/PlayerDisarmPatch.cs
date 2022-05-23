using System;
using HarmonyLib;
using InventorySystem.Disarming;
using InventorySystem.Items;
using Mirror;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(DisarmingHandlers), nameof(DisarmingHandlers.ServerProcessDisarmMessage))]
    internal static class PlayerDisarmPatch
    {
        [HarmonyPrefix]
        private static bool OnDisarm(NetworkConnection conn, DisarmMessage msg)
        {
            try
            {
                var cuffer = conn.GetPlayer();
                var target = msg.PlayerToDisarm?.GetPlayer();

                if (!msg.PlayerIsNull)
                {
                    if ((msg.PlayerToDisarm.transform.position - cuffer.Position).sqrMagnitude > 20f)
                        return false;

                    if (msg.PlayerToDisarm.inventory.CurInstance is not null && msg.PlayerToDisarm.inventory.CurInstance.TierFlags is not ItemTierFlags.Common)
                        return false;
                }

                var flag = !msg.PlayerIsNull && msg.PlayerToDisarm.inventory.IsDisarmed();
                var flag2 = !msg.PlayerIsNull && DisarmedPlayers.CanDisarm(cuffer.Hub, msg.PlayerToDisarm);

                if (flag && !msg.Disarm)
                {
                    if (!cuffer.IsCuffed)
                    {
                        SynapseController.Server.Events.Player.InvokeUncuff(cuffer, target, out var allow);
                        if (!allow) return false;
                        msg.PlayerToDisarm.inventory.SetDisarmedStatus(null);
                    }
                }
                else
                {
                    if (flag || !flag2 || !msg.Disarm)
                    {
                        cuffer.NetworkIdentity.connectionToClient.Send(DisarmingHandlers.NewDisarmedList, 0);
                        return false;
                    }
                    if (msg.PlayerToDisarm.inventory.CurInstance is null || msg.PlayerToDisarm.inventory.CurInstance.CanHolster())
                    {
                        SynapseController.Server.Events.Player.InvokePlayerCuffTargetEvent(target, cuffer, out var allow2);

                        if (!allow2) return false;

                        msg.PlayerToDisarm.inventory.SetDisarmedStatus(cuffer.VanillaInventory);
                    }
                }

                NetworkServer.SendToAll(DisarmingHandlers.NewDisarmedList, 0, false);
                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerCuffTarget event failed!!\n{e}");
                return true;
            }
        }
    }
}