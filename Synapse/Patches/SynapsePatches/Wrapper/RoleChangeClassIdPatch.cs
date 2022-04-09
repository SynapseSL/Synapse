using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDHook))]
    internal static class RoleChangeClassIdPatch
    {
        internal static bool ForceLite = false;

        [HarmonyPrefix]
        private static bool SetClass(CharacterClassManager __instance, RoleType id)
        {
            __instance.SetClassIDAdv(id, ForceLite, CharacterClassManager.SpawnReason.None, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetClassID))]
    internal static class RoleChangeClassIdPatch2
    {
        [HarmonyPrefix]
        private static bool SetClass(CharacterClassManager __instance, RoleType id, CharacterClassManager.SpawnReason spawnReason)
        {
            __instance.SetClassIDAdv(id, RoleChangeClassIdPatch.ForceLite, spawnReason, false);
            return false;
        }
    }
}