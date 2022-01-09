using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new Type[] { typeof(Team), typeof(Team), typeof(bool) })]

    internal static class CheckFriendlyFirePatch
    {
        [HarmonyPrefix]
        private static bool CheckFriendlyFire(Team attackerTeam, Team victimTeam, bool ignoreConfig, out bool __result)
        {
            __result = true;
            return false;
        }
    }
}
