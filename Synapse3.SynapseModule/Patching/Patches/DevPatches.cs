﻿namespace Synapse3.SynapseModule.Patching.Patches;

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
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Coin;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Voice;
using RelativePositioning;
using Synapse3.SynapseModule.Dummy;

[Automatic]
[SynapsePatch("Debug", PatchType.Dev)]
public static class TestPatches
{

}
#endif