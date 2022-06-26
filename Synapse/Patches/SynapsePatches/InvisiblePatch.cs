using CustomPlayerEffects;
using HarmonyLib;
using PlayableScps;
using System;
using System.Linq;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    internal static class InvisiblePatch
    {
        [HarmonyPrefix]
        private static bool TransmitData(PlayerPositionManager __instance)
        {
            try
            {
                __instance._frame++;

                if (__instance._frame != __instance._syncFrequency)
                    return false;

                __instance._frame = 0;

                var players = Server.Get.Players;
                players.AddRange(Synapse.Api.Map.Get.Dummies.Select(x => x.Player));
                __instance._usedData = players.Count;

                if (__instance.ReceivedData is null || __instance.ReceivedData.Length < __instance._usedData)
                    __instance.ReceivedData = new PlayerPositionData[__instance._usedData * 2];

                for (var i = 0; i < __instance._usedData; i++)
                    __instance.ReceivedData[i] = new PlayerPositionData(players[i].Hub);

                if (__instance._transmitBuffer is null || __instance._transmitBuffer.Length < __instance._usedData)
                    __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];

                foreach (var player in players)
                {
                    if (player.Connection is null)
                        continue;

                    Array.Copy(__instance.ReceivedData, __instance._transmitBuffer, __instance._usedData);
                    for (var k = 0; k < __instance._usedData; k++)
                    {
                        var showinvoid = false;
                        var playerToShow = players[k];
                        if (playerToShow == player)
                            continue;

                        var vector = __instance._transmitBuffer[k].position - player.Position;

                        if (player.RoleType == RoleType.Spectator)
                            continue;

                        if (player.RoleType == RoleType.Scp173)
                        {
                            if (player.Scp173Controller.IgnoredPlayers.Contains(playerToShow))
                            {
                                showinvoid = true;
                                goto AA_001;
                            }
                            else if ((playerToShow.RealTeam == Team.SCP && player.CustomRole != null && !Server.Get.Configs.SynapseConfiguration.ScpTrigger173) || Server.Get.Configs.SynapseConfiguration.CantLookAt173.Contains(playerToShow.RoleID) || player.Scp173Controller.TurnedPlayers.Contains(playerToShow) || playerToShow.Invisible)
                            {
                                var posinfo = __instance._transmitBuffer[k];
                                var rot = Quaternion.LookRotation(playerToShow.Position - player.Position).eulerAngles.y;
                                __instance._transmitBuffer[k] = new PlayerPositionData(posinfo.position, rot, posinfo.playerID);
                            }
                        }
                        else if (player.RoleID == (int)RoleType.Scp93953 || player.RoleID == (int)RoleType.Scp93989)
                        {
                            if (__instance._transmitBuffer[k].position.y < 800f && SynapseExtensions.CanHarmScp(playerToShow, false) && playerToShow.RealTeam != Team.RIP && !playerToShow.GetComponent<Scp939_VisionController>().CanSee(player.PlayerEffectsController.GetEffect<CustomPlayerEffects.Visuals939>()))
                            {
                                showinvoid = true;
                                goto AA_001;
                            }
                        }

                        if (playerToShow.Invisible && !player.HasPermission("synapse.see.invisible"))
                        {
                            showinvoid = true;
                            goto AA_001;
                        }

                        if (Math.Abs(vector.y) > 35f)
                        {
                            showinvoid = true;
                            goto AA_001;
                        }
                        else
                        {
                            var sqrMagnitude = vector.sqrMagnitude;
                            if (player.Position.y < 800f)
                            {
                                if (sqrMagnitude >= 1764f)
                                {
                                    showinvoid = true;
                                    goto AA_001;
                                }
                            }
                            else if (sqrMagnitude >= 7225f)
                            {
                                showinvoid = true;
                                goto AA_001;
                            }

                            if (playerToShow != null)
                            {
                                var scp = player.Hub.scpsController.CurrentScp as Scp096;

                                if (scp != null && scp.Enraged && !scp.HasTarget(playerToShow.Hub) && SynapseExtensions.CanHarmScp(playerToShow, false))
                                {
                                    showinvoid = true;
                                    goto AA_001;
                                }

                                if (playerToShow.Hub.playerEffectsController.GetEffect<Invisible>().IsEnabled)
                                {
                                    var flag = false;
                                    if (scp != null)
                                        flag = scp.HasTarget(playerToShow.Hub);

                                    if (player.RoleType == RoleType.Scp079 || flag)
                                    {
                                        if (Server.Get.Configs.SynapseConfiguration.Better268)
                                            showinvoid = true;
                                    }
                                    else
                                    {
                                        showinvoid = true;
                                    }
                                }
                            }
                        }

                    AA_001:

                        var posData = __instance._transmitBuffer[k];
                        var rotation = posData.rotation;
                        var pos = posData.position;

                        Server.Get.Events.Server.InvokeTransmitPlayerDataEvent(player, playerToShow, ref pos, ref rotation, ref showinvoid);

                        __instance._transmitBuffer[k] = showinvoid
                            ? new PlayerPositionData(Vector3.up * 6000f, 0.0f, playerToShow.PlayerId)
                            : new PlayerPositionData(pos, rotation, playerToShow.PlayerId);
                    }

                    var conn = player.Connection;
                    if (__instance._usedData <= 20)
                    {
                        conn.Send(new PositionPPMMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
                    }
                    else
                    {
                        byte b = 0;
                        while (b < __instance._usedData / 20)
                        {
                            conn.Send(new PositionPPMMessage(__instance._transmitBuffer, 20, b), 1);
                            b += 1;
                        }

                        var b2 = (byte)(__instance._usedData % (b * 20));

                        if (b2 > 0)
                            conn.Send(new PositionPPMMessage(__instance._transmitBuffer, b2, b), 1);
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Api.Logger.Get.Error($"Synapse-InvisibleMode: TransmitData failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp173), nameof(Scp173.UpdateObservers))]
    internal static class PeanutPatch
    {
        private static void Prefix(Scp173 __instance, out int __state) => __state = __instance._observingPlayers.Count;

        private static void Postfix(Scp173 __instance, int __state)
        {
            var peanut = __instance.GetPlayer();
            
            peanut.Scp173Controller.ConfrontingPlayers.Clear();
            foreach (var ply in __instance._observingPlayers)
            {
                var player = ply.GetPlayer();
                var flag = false;

                peanut.Scp173Controller.ConfrontingPlayers.Add(player);

                if (player.Invisible || (player.RealTeam == Team.SCP && !Server.Get.Configs.SynapseConfiguration.ScpTrigger173) || Server.Get.Configs.SynapseConfiguration.CantLookAt173.Contains(player.RoleID))
                    flag = true;

                if (peanut.Scp173Controller.IgnoredPlayers.Contains(player) || player.Scp173Controller.TurnedPlayers.Contains(player))
                    flag = true;

                if (flag)
                {
                    __instance._observingPlayers.Remove(player.Hub);
                    __instance._isObserved = __instance._observingPlayers.Count > 0;
                    if (__state != __instance._observingPlayers.Count && __instance._blinkCooldownRemaining > 0f)
                    {
                        __instance._blinkCooldownRemaining = Mathf.Max(0f, __instance._blinkCooldownRemaining + (__instance._observingPlayers.Count - __state));
                        if (__instance._blinkCooldownRemaining <= 0f)
                            __instance.BlinkReady = true;
                    }
                }
            }
        }
    }
}
