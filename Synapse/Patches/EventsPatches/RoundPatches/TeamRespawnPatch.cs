using HarmonyLib;
using Respawning;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NorthwoodLib.Pools;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    [HarmonyPatch(typeof(RespawnManager),nameof(RespawnManager.Spawn))]
    internal static class TeamRespawnPatch
    {
        private static bool Prefix(RespawnManager __instance)
        {
            try
            {
				if (!RespawnWaveGenerator.SpawnableTeams.TryGetValue(__instance.NextKnownTeam, out var spawnableTeam) || __instance.NextKnownTeam == SpawnableTeamType.None)
				{
					ServerConsole.AddLog("Fatal error. Team '" + __instance.NextKnownTeam + "' is undefined.", ConsoleColor.Red);
					return false;
				}
				List<ReferenceHub> list = (from item in ReferenceHub.GetAllHubs().Values
										   where item.characterClassManager.CurClass == RoleType.Spectator && !item.serverRoles.OverwatchEnabled
										   select item).ToList<ReferenceHub>();

				if (__instance._prioritySpawn)
				{
					list = (from item in list
							orderby item.characterClassManager.DeathTime
							select item).ToList<ReferenceHub>();
				}
				else
					list.ShuffleList<ReferenceHub>();

				var num = RespawnTickets.Singleton.GetAvailableTickets(__instance.NextKnownTeam);

				if (num == 0)
				{
					num = 5;
					RespawnTickets.Singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, 5, true);
				}
				var num2 = Mathf.Min(num, spawnableTeam.MaxWaveSize);

				while (list.Count > num2)
					list.RemoveAt(list.Count - 1);

				list.ShuffleList();

				var list2 = ListPool<ReferenceHub>.Shared.Rent();

				var players = list.Select(x => x.GetPlayer()).ToList();
				var team = __instance.NextKnownTeam;

				SynapseController.Server.Events.Round.InvokeTeamRespawn(ref players, ref team, out var allow, out var id);

				if (!allow) return false;

				if(team == SpawnableTeamType.None)
                {
					Server.Get.TeamManager.SpawnTeam(id,players);
					return false;
                }

				list = players.Select(x => x.Hub).ToList();
				__instance.NextKnownTeam = team;

				foreach (ReferenceHub referenceHub in list)
				{
					try
					{
						RoleType classid = spawnableTeam.ClassQueue[Mathf.Min(list2.Count, spawnableTeam.ClassQueue.Length - 1)];
						referenceHub.characterClassManager.SetPlayersClass(classid, referenceHub.gameObject, false, false);
						list2.Add(referenceHub);
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
						{
				"Player ",
				referenceHub.LoggedNameFromRefHub(),
				" respawned as ",
				classid.ToString(),
				"."
						}), ServerLogs.ServerLogType.GameEvent, false);
					}
					catch (Exception ex)
					{
						if (referenceHub != null)
						{
							ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Player " + referenceHub.LoggedNameFromRefHub() + " couldn't be spawned. Err msg: " + ex.Message, ServerLogs.ServerLogType.GameEvent, false);
						}
						else
						{
							ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Couldn't spawn a player - target's ReferenceHub is null.", ServerLogs.ServerLogType.GameEvent, false);
						}
					}
				}
				if (list2.Count > 0)
				{
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new object[]
					{
						"RespawnManager has successfully spawned ",
			            list2.Count,
			            " players as ",
			           __instance.NextKnownTeam.ToString(),
			          "!"
					}), ServerLogs.ServerLogType.GameEvent, false);
					RespawnTickets.Singleton.GrantTickets(__instance.NextKnownTeam, -list2.Count * spawnableTeam.TicketRespawnCost, false);
                    if (Respawning.NamingRules.UnitNamingRules.TryGetNamingRule(__instance.NextKnownTeam, out var unitNamingRule))
                    {
                        unitNamingRule.GenerateNew(__instance.NextKnownTeam, out string text);
                        foreach (ReferenceHub referenceHub2 in list2)
                        {
                            referenceHub2.characterClassManager.NetworkCurSpawnableTeamType = (byte)__instance.NextKnownTeam;
                            referenceHub2.characterClassManager.NetworkCurUnitName = text;
                        }
                        unitNamingRule.PlayEntranceAnnouncement(text);
                    }
                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, __instance.NextKnownTeam);
				}
				ListPool<ReferenceHub>.Shared.Return(list2);
				__instance.NextKnownTeam = SpawnableTeamType.None;

				return false;
			}
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: TeamRespawn failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
