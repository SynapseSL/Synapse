using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetRandomRoles))]
    internal static class SpawnPlayersPatch
    {
        private static bool Prefix(CharacterClassManager __instance, bool first)
        {
            try
            {
                if(__instance.isLocalPlayer && __instance.isServer)
                {
                    __instance.RunDefaultClassPicker(first, out var roleList, out var dictionary);

                    ServerLogs.AddLog(ServerLogs.Modules.Logger, "Random classes have been assigned by DCP.", ServerLogs.ServerLogType.GameEvent, false);

                    if(first && GameCore.ConfigFile.ServerConfig.GetBool("smart_class_picker", true) && GameCore.Console.EnableSCP)
                    {
                        __instance.RunSmartClassPicker(roleList, out dictionary);

                        ServerLogs.AddLog(ServerLogs.Modules.Logger, "Smart class picking has been performed.", ServerLogs.ServerLogType.GameEvent, false);
                    }

                    var newDictionary = new Dictionary<Player, int>();
                    foreach (var pair in dictionary)
                        if(pair.Key.GetPlayer() != Server.Get.Host)
                        newDictionary.Add(pair.Key.GetPlayer(), (int)pair.Value);

                    Server.Get.Events.Round.InvokeSpawnPlayersEvent(ref newDictionary, out var allow);

                    if (allow)
                    {
                        var builder = StringBuilderPool.Shared.Rent();
                        foreach (var pair in newDictionary)
                        {
                            pair.Key.RoleID = pair.Value;

                            builder.Append((RoleType)pair.Value + " | ");
                        }

                        ServerLogs.AddLog(ServerLogs.Modules.Logger, "Class Picker Result: " + builder, ServerLogs.ServerLogType.GameEvent, false);
                        StringBuilderPool.Shared.Return(builder);
                    }
                }
                if (NetworkServer.active)
                    Timing.RunCoroutine(__instance.MakeSureToSetHPAndStamina(),Segment.FixedUpdate);

                return false;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: SpawnPlayers failed!!\n{e}");
                return true;
            }
        }
    }
}
