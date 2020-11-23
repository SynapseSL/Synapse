using System;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches.Wrapper
{
    [HarmonyPatch(typeof(Scp914.Scp914Machine),nameof(Scp914.Scp914Machine.Start))]
    internal static class Scp914StartPatch
    {
        private static bool loaded = false;

        private static void Prefix(Scp914.Scp914Machine __instance)
        {
            try
            {
                if (loaded) return;

                foreach (var recipe in __instance.recipes)
                    Synapse.Api.Map.Get.Scp914.Recipes.Add(new Api.Scp914Recipe(recipe));

                loaded = true;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Wrapper: Scp914 recipes failed!!\n{e}");
            }
        }
    }
}
