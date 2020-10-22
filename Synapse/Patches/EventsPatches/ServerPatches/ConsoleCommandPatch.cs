using System;
using RemoteAdmin;

namespace Synapse.Patches.EventsPatches.ServerPatches
{
    internal static class ConsoleCommandPatch
    {
        private static void Prefix(QueryProcessor __instance, string query)
        {
            try
            {
                SynapseController.Server.Events.Server.InvokeConsoleCommandEvent(__instance.GetPlayer(), query);
            }
            catch (Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: ConsoleCommandEvent failed!!\n{e}");
            }
        }
    }
}