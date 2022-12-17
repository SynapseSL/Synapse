using HarmonyLib;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("PlayerEscape", PatchType.PlayerEvent)]
public static class PlayerEscapePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Escape), nameof(Escape.ServerHandlePlayer))]
    public static bool OnEscape(ReferenceHub hub)
    {
        var player = hub.GetSynapsePlayer();
        if (player == null) return false;
        player.TriggerEscape(false);
        return false;
    }
}