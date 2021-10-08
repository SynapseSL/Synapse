using HarmonyLib;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.UserCode_CmdRegisterEscape))]
    internal static class PlayerEscapePatch
    {
        //So the Client does only send the Escape Command the first time he is at the exit as Scientiest/D-Personnel so other Roles cant escape or if you replace a D-Personnel/Scientist and try to escape a second time.
        //In order to fix this we block this Command entirely and do everything on our own.
        [HarmonyPrefix]
        private static bool RegisterEscape() => false;
    }
}