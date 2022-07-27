using System;
using HarmonyLib;
using Neuron.Core.Logging;
using Respawning;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.SynapseModule.Patches;

[Patches]
internal static class TeamPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnManager),nameof(RespawnManager.Update))]
    public static bool OnTeamUpdate(RespawnManager __instance)
    {
        try
        {
            if (!__instance.ReadyToCommence()) 
                return false;

            if (__instance._stopwatch.Elapsed.TotalSeconds > __instance._timeForNextSequence)
                __instance._curSequence += 1;

            var service = Synapse.Get<TeamService>();

            switch (__instance._curSequence)
            {
                case RespawnManager.RespawnSequencePhase.SelectingTeam:
                    var nextTeam = (int)RespawnTickets.Singleton.DrawRandomTeam();

                    var ev = new SelectTeamEvent()
                    {
                        TeamId = nextTeam
                    };
                    Synapse.Get<RoundEvents>().SelectTeam.Raise(ev);
                    nextTeam = ev.TeamId;

                    if (ev.Reset || nextTeam == 0)
                    {
                        __instance.RestartSequence();
                        return false;
                    }
                    service.NextTeam = nextTeam;
                    __instance._curSequence = RespawnManager.RespawnSequencePhase.PlayingEntryAnimations;
                    __instance._stopwatch.Restart();
                    __instance._timeForNextSequence = service.GetRespawnTime(nextTeam);
                    service.ExecuteRespawnAnnouncement(nextTeam);
                    break;
                    
                case RespawnManager.RespawnSequencePhase.SpawningSelectedTeam:
                    service.Spawn();
                    __instance.RestartSequence();
                    break;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: SelectTeam Event failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.ForceSpawnTeam))]
    public static bool ForceRespawn(RespawnManager __instance, SpawnableTeamType teamToSpawn)
    {
        try
        {
            var service = Synapse.Get<TeamService>();
            service.NextTeam = (int)teamToSpawn;
            service.Spawn();
            __instance.RestartSequence();
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Team: Force Team Respawn failed\n" + ex);
            return false;
        }
    }
}