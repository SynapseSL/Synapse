using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.GetAllHubs))]
    internal static class GetAllHubsPatch
    {
        [HarmonyPrefix]
        private static bool OnGetAllHubs(out Dictionary<GameObject, ReferenceHub> __result)
        {
            __result = new();

            //This will remove all dummies
            foreach (var pair in ReferenceHub.Hubs)
                if (pair.Value.networkIdentity?.connectionToClient is not null)
                    __result.Add(pair.Key, pair.Value);

            return false;
        }
    }
}