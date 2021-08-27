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
}