using HarmonyLib;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetClassID))]
    public class RoleChangeClassIdPatch
    {
        internal static bool ForceLite = false;

        public static bool Prefix(CharacterClassManager __instance, RoleType id)
        {
            __instance.SetClassIDAdv(id, ForceLite, false);
            ForceLite = false;
            return false;
        }
    }
}