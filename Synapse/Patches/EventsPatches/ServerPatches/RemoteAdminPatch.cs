using System;
using HarmonyLib;
using RemoteAdmin;

namespace Synapse.Patches.EventsPatches.ServerPatches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery), typeof(string), typeof(CommandSender))]
    internal static class RemoteAdminPatch
    {
        private static bool Prefix(ref string q, ref CommandSender sender)
        {
            try
            {
                if (q.Contains("REQUEST_DATA PLAYER_LIST SILENT")) return true;

                var allow = true;
                SynapseController.Server.Events.Server.InvokeRemoteAdminCommandEvent(sender, q, ref allow);

                return allow;
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: RemoteAdminEvent failed!!\n{e}");
                return true;
            }
        }
    }
}