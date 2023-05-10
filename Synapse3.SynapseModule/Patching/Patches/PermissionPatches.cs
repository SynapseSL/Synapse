using System;
using HarmonyLib;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Patching.Patches;

//#if !PATCHLESS
[Automatic]
[SynapsePatch("RefreshPermission", PatchType.Permission)]
public static class RefreshPermissionPatch
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshPermissions)), HarmonyPrefix]
    public static bool RefreshPermission(ServerRoles __instance, bool disp = false)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            player?.RefreshPermission(disp);
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Permission: RefreshPermissionPatch failed!!\n{e}");
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("SetPermissionGroup", PatchType.Permission)]
public static class SetPermissionPatch
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetGroup)), HarmonyPrefix]
    public static bool SetGroup() => false;
}
//#endif