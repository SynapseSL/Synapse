using System;
using System.Collections.Generic;
using HarmonyLib;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using Synapse.Api;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(CharacterClassManager),nameof(CharacterClassManager.SetRandomRoles))]
    internal static class SpawnPlayersPatch
    {
        [HarmonyPrefix]
        private static bool SetRandomRoles(CharacterClassManager __instance, bool first)
        {
            try
            {
                __instance.RunDefaultClassPicker(first, out var roleList, out var dictionary);

                ServerLogs.AddLog(ServerLogs.Modules.Logger, "Random classes have been assigned by DCP.", ServerLogs.ServerLogType.GameEvent, false);

                if (first && GameCore.ConfigFile.ServerConfig.GetBool("smart_class_picker", true) && GameCore.Console.EnableSCP)
                {
                    __instance.RunSmartClassPicker(roleList, out dictionary);

                    ServerLogs.AddLog(ServerLogs.Modules.Logger, "Smart class picking has been performed.", ServerLogs.ServerLogType.GameEvent, false);
                }

                var newDictionary = new Dictionary<Player, int>();
                foreach (var pair in dictionary)
                {
                    var player = pair.Key.GetPlayer();
                    if (player == Server.Get.Host) continue;

                    //This fix the Bug that Overwatch Players get spawned in for a sec and then get sets to Spectator again which results in many bugs
                    if (player.OverWatch)
                    {
                        player.RoleID = (int)RoleType.Spectator;
                        continue;
                    }

                    if (__instance.IsCIOnStart && pair.Value == RoleType.FacilityGuard)
                        newDictionary.Add(player, (int)RoleType.ChaosRifleman);
                    else
                        newDictionary.Add(player, (int)pair.Value);
                }

                var allow = true;
                try
                {
                    Server.Get.Events.Round.InvokeSpawnPlayersEvent(ref newDictionary, out allow);
                }
                catch (Exception e)
                {
                    Logger.Get.Error($"Synapse-Event: SpawnPlayersEvent failed!!\n{e}");
                }

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
