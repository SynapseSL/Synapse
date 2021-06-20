using HarmonyLib;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(ServerRoles),"get_KickPower")]
    internal static class GlobalKickPowerPatch
    {
        private static void Postfix(ref byte __result, ServerRoles __instance)
        {
            var ply = __instance.GetPlayer();
            if (ply.GlobalSynapseGroup != null && (ply.GlobalSynapseGroup.Kick || ply.GlobalSynapseGroup.Ban))
                __result = byte.MaxValue;
        }
    }
}
