﻿using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Generator079),nameof(Generator079.EjectTablet))]
    internal static class GeneratorEjectPatch
    {
        private static bool Prefix(Generator079 __instance)
        {
            if (__instance.isTabletConnected)
            {
                var gen = __instance.GetGenerator();
                __instance.NetworkisTabletConnected = false;
                if (gen.ConnectedTablet != null)
                    gen.ConnectedTablet.Drop(gen.TabletEjectionPoint);
            }
            return false;
        }
    }
}
