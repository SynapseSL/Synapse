using HarmonyLib;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDHook))]
    internal static class RoleChangeClassIdPatch
    {
        [HarmonyPrefix]
        private static bool SetClass(CharacterClassManager __instance, RoleType id)
        {
            var player = __instance.GetPlayer();
            __instance.SetClassIDAdv(id, player.LiteRoleSet, CharacterClassManager.SpawnReason.None, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetClassID))]
    internal static class RoleChangeClassIdPatch2
    {
        [HarmonyPrefix]
        private static bool SetClass(CharacterClassManager __instance, RoleType id, CharacterClassManager.SpawnReason spawnReason)
        {
            var player = __instance.GetPlayer();
            __instance.SetClassIDAdv(id, player.LiteRoleSet, spawnReason, false);
            return false;
        }
    }
}