using HarmonyLib;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassIDHook))]
    public class RoleChangeClassIdPatch
    {
        internal static bool ForceLite = false;

        public static bool Prefix(CharacterClassManager __instance, RoleType id)
        {
            __instance.SetClassIDAdv(id, ForceLite, CharacterClassManager.SpawnReason.None, true);
            return false;
        }
    }
}