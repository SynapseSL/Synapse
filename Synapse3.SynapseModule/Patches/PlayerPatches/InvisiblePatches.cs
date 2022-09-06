using System;
using System.Linq;
using CustomPlayerEffects;
using HarmonyLib;
using Neuron.Core.Logging;
using PlayableScps;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Patches.PlayerPatches;

[Patches]
[HarmonyPatch]
internal static class InvisiblePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    public static bool TransmitData(PlayerPositionManager __instance)
    {
        try
        {
            __instance._frame++;
            if (__instance._frame != __instance._syncFrequency)
                return false;
            __instance._frame = 0;

            var service = Synapse.Get<PlayerService>();
            var players = service.Players.ToList();
            players.AddRange(Synapse.Get<DummyService>().Dummies.Where(x => x.PlayerUpdate).Select(x => x.Player));
            __instance._usedData = players.Count;

            if (__instance.ReceivedData == null || __instance.ReceivedData.Length < __instance._usedData)
            {
                __instance.ReceivedData = new PlayerPositionData[__instance._usedData * 2];
            }

            for (int i = 0; i < __instance._usedData; i++)
            {
                __instance.ReceivedData[i] = new PlayerPositionData(players[i]);
            }

            if (__instance._transmitBuffer == null || __instance._transmitBuffer.Length < __instance._usedData)
            {
                __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];
            }

            foreach (var player in players)
            {
                if(player.PlayerType != PlayerType.Player) continue;

                Array.Copy(__instance.ReceivedData, __instance._transmitBuffer, __instance._usedData);

                for (int i = 0; i < __instance._usedData; i++)
                {
                    var invisible = false;
                    var playerToShow = players[i];
                    var isOnSurface = (player.Room as SynapseRoom)?.RoomType == RoomType.Surface;
                    
                    if (player == playerToShow || playerToShow.RoleType is RoleType.Spectator) continue;

                    switch (playerToShow.Invisible)
                    {
                        case InvisibleMode.Visual when player.RoleType is not RoleType.Scp079 and not RoleType.Scp93953 and not RoleType.Scp93989 and not RoleType.Scp096 and not RoleType.Spectator:
                        case InvisibleMode.Alive or InvisibleMode.Ghost when player.RoleType != RoleType.Spectator:
                        case InvisibleMode.Admin when !player.HasPermission("synapse.invisible"):
                        case InvisibleMode.Full:
                            invisible = true;
                            goto ExecuteEnd;
                    }

                    switch (player.RoleType)
                    {
                        //Spectators should be able to see everyone therefore any other checks like Invisible Effect will be skipped for them
                        case RoleType.Spectator: continue;
                        
                        case RoleType.Scp93953 or RoleType.Scp93989:
                        {
                            if (!isOnSurface && Synapse3Extensions.CanHarmScp(playerToShow, false) &&
                                !playerToShow.Scp939VisionController.CanSee(player.GetEffect<Visuals939>()))
                            {
                                invisible = true;
                                goto ExecuteEnd;
                            }

                            break;
                        }
                        
                        case RoleType.Scp096 when playerToShow.Invisible == InvisibleMode.Visual:
                            if (player.ScpController.Scp096.Is096Instance &&
                                player.ScpController.Scp096.Scp096.EnragedOrEnraging &&
                                player.ScpController.Scp096.Targets.Contains(playerToShow))
                                break;
                            invisible = true;
                            goto ExecuteEnd;
                    }

                    //SCP-096 bypasses mostly any invisible checks from here on out so that he can see everyone anywhere even when he is enraged
                    var scp = playerToShow.ScpsController.CurrentScp as Scp096;

                    //There are a lot of checks related to the Position to see if the player needs to know where the other player is.
                    //However when both players are in the same room these are irrelevant anyways and only causes problems in huge rooms like the surface
                    if (playerToShow.Room != player.Room)
                    {
                        var distance = playerToShow.Position - player.Position;
                        var square = distance.sqrMagnitude;
                        
                        if (Math.Abs(distance.y) > 35)
                        {
                            invisible = true;
                        }
                        else if (square >= 22500f)
                        {
                            invisible = true;
                        }
                    }

                    if (playerToShow.GetEffect<Invisible>().IsEnabled)
                    {
                        if (Synapse.Get<SynapseConfigService>().GamePlayConfiguration.BetterScp268)
                        {
                            invisible = true;
                            goto ExecuteEnd;
                        }
                        
                        if (player.RoleType is not RoleType.Scp079 or RoleType.Spectator)
                        {
                            invisible = true;
                        }
                    }

                    if (scp?.EnragedOrEnraging == true)
                    {
                        invisible = !scp.HasTarget(playerToShow);
                    }

                    ExecuteEnd:

                    if (invisible)
                    {
                        __instance._transmitBuffer[i] =
                            new PlayerPositionData(Vector3.up * 7000f, 0.0f, playerToShow.PlayerId);
                    }
                }

                if (__instance._usedData <= 20)
                {
                    player.SendNetworkMessage(
                        new PositionPPMMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
                }
                else
                {
                    byte b = 0;
                    while (b < __instance._usedData / 20)
                    {
                        player.SendNetworkMessage(new PositionPPMMessage(__instance._transmitBuffer, 20, b), 1);
                        b++;
                    }

                    var b2 = (byte)(__instance._usedData / (b * 20));

                    if (b2 > 0)
                        player.SendNetworkMessage(new PositionPPMMessage(__instance._transmitBuffer, b2, b), 1);
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: TransmitData Patch failed\n" + ex);
            return true;
        }
    }
}