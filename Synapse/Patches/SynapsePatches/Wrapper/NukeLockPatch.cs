using HarmonyLib;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(PlayerInteract),nameof(PlayerInteract.UserCode_CmdUsePanel))]
    internal static class NukeLockPatch
    {
        private static bool Prefix() => !Map.Get.Nuke.InsidePanel.Locked;
    }
}
