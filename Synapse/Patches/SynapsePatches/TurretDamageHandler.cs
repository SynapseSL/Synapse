using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using Synapse.Api;
using System;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(DamageHandlerReaderWriter), nameof(DamageHandlerReaderWriter.WriteDamageHandler))]
    internal static class TurretDamageHandler
    {
        [HarmonyPrefix]
        private static bool WriteDamageHandler(NetworkWriter writer, DamageHandlerBase info)
        {
            try
            {
                if (info?.GetType() == typeof(Turret.SynapseTurretDamageHandler))
                {
                    writer?.WriteByte(DamageHandlers.IdsByTypeHash[typeof(CustomReasonDamageHandler).FullName.GetStableHashCode()]);
                    info?.WriteAdditionalData(writer);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Get.Error("Synapse-Turret: Failed to write Data of SynapseTurretDamageHandler:\n" + ex);
                return true;
            }
        }
    }
}