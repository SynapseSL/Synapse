namespace Synapse3.SynapseModule.Patching.Patches;

#if DEV
using System;
using HarmonyLib;
using Neuron.Core.Meta;

[Automatic]
[SynapsePatch("No ServerList",PatchType.Dev)]
public static class DevPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RunServer))]
    public static bool OnVerification()
    {
        ServerConsole.AddLog("Server WON'T be visible on the public list due to usage of a Synapse Dev Version. This Version is only intended to be used for developers and not verified servers!",ConsoleColor.DarkRed);
        return false;
    }
}
#endif

#if DEBUG
using System;
using HarmonyLib;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Voice;
using RelativePositioning;
using Synapse3.SynapseModule.Dummy;

[Automatic]
[SynapsePatch("Debug", PatchType.Dev)]
public static class TestPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(VoiceChatReceivePrefs), nameof(VoiceChatReceivePrefs.GetFlagsForUser))]
    public static bool GetFlagsForUser(GroupMuteFlags __result, ReferenceHub hub)
    {
        var player = hub.GetSynapsePlayer();
        if (player is DummyPlayer)
        {
            __result = GroupMuteFlags.None;
            return false;
        }
        return true;
    }
}

[Automatic]
[SynapsePatch("Rotation Debug", PatchType.Dev)]
public static class TestPatches2
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FpcSyncData), nameof(FpcSyncData.Write))]
    public static bool GetRotForUser(FpcSyncData __instance, NetworkWriter writer)
    {
        Misc.ByteToBools((byte)__instance._state, out var bool1, out var bool2, out var bool3, out var bool4, out var bool5, out var _, out var _, out var _);
        writer.WriteByte(Misc.BoolsToByte(bool1, bool2, bool3, bool4, bool5, __instance._bitMouseLook, __instance._bitPosition, __instance._bitCustom));
        if (__instance._bitPosition)
        {
            writer.WriteRelativePosition(__instance._position);
        }

        ServerConsole.AddLog("_bitMouseLook: " + __instance._bitMouseLook);
        if (__instance._bitMouseLook || true)
        {
            writer.WriteUInt16(__instance._rotH);
            writer.WriteUInt16(__instance._rotV);
        }
        return false;
    }
}
#endif