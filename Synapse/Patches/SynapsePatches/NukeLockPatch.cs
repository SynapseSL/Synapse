using Harmony;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(PlayerInteract),nameof(PlayerInteract.CallCmdUsePanel))]
    internal static class NukeLockPatch
    {
        private static bool Prefix() => Map.Get.Nuke.InsidePanel.Locked;
    }
}
