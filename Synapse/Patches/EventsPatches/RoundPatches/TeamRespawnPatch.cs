using HarmonyLib;
using Respawning;
using System;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    //[HarmonyPatch(typeof(RespawnManager),nameof(RespawnManager.Spawn))]
    internal static class TeamRespawnPatch
    {
        private static bool Prefix(RespawnManager __instance)
        {
            try
            {

                return false;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: TeamRespawn failed!!\n{e}");
                return true;
            }
        }
    }
}
