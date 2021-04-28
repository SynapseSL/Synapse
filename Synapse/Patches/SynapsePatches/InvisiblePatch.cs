using CustomPlayerEffects;
using HarmonyLib;
using Mirror;
using PlayableScps;
using System;
using UnityEngine;
using System.Linq;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    internal static class InvisiblePatch
    {
        private static bool Prefix(PlayerPositionManager __instance)
        {
            try
            {
                if (!NetworkServer.active)
                    return false;

                __instance._frame++;

                if (__instance._frame != __instance._syncFrequency)
                    return false;

                __instance._frame = 0;

                var players = Server.Get.Players;
                players.AddRange(Synapse.Api.Map.Get.Dummies.Select(x => x.Player));
                __instance._usedData = players.Count;

                if (__instance._receivedData == null || __instance._receivedData.Length < __instance._usedData)
                    __instance._receivedData = new PlayerPositionData[__instance._usedData * 2];

                for (var i = 0; i < __instance._usedData; i++)
                    __instance._receivedData[i] = new PlayerPositionData(players[i].Hub);

                if (__instance._transmitBuffer == null || __instance._transmitBuffer.Length < __instance._usedData)
                    __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];

                foreach (var player in players)
                {
                    if (player.Connection == null) continue;

                    Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);
                    for (int k = 0; k < __instance._usedData; k++)
                    {
                        var showinvoid = false;
                        var playerToShow = players[k];
                        var vector = __instance._transmitBuffer[k].position - player.Position;

                        if (player.RoleType == RoleType.Scp173)
                        {
                            if (player.Scp173Controller.IgnoredPlayers.Contains(playerToShow))
                            {
                                showinvoid = true;
                                goto AA_001;
                            }
                            else if ((playerToShow.RealTeam == Team.SCP && !Server.Get.Configs.synapseConfiguration.ScpTrigger173) || Server.Get.Configs.synapseConfiguration.CantLookAt173.Contains(playerToShow.RoleID) || player.Scp173Controller.TurnedPlayers.Contains(playerToShow) || playerToShow.Invisible)
                            {
                                var posinfo = __instance._transmitBuffer[k];
                                var rot = Quaternion.LookRotation(playerToShow.Position - player.Position).eulerAngles.y;
                                __instance._transmitBuffer[k] = new PlayerPositionData(posinfo.position, rot, posinfo.playerID);
                            }
                        }
                        else if (player.RoleID == (int)RoleType.Scp93953 || player.RoleID == (int)RoleType.Scp93989)
                        {
                            if (__instance._transmitBuffer[k].position.y < 800f && SynapseExtensions.CanHarmScp(playerToShow, false) && playerToShow.RealTeam != Team.RIP && !playerToShow.GetComponent<Scp939_VisionController>().CanSee(player.ClassManager.Scp939))
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

                        if (player.RoleType == RoleType.Spectator)
                            continue;

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
                                if (playerToShow.Hub.playerEffectsController.GetEffect<Scp268>().Enabled)
                                {
                                    var flag = false;
                                    if (scp != null)
                                        flag = scp.HasTarget(playerToShow.Hub);

                                    if (player.RoleType == RoleType.Scp079 || flag)
                                    {
                                        if (Server.Get.Configs.synapseConfiguration.Better268)
                                            showinvoid = true;
                                    }
                                    else
                                        showinvoid = true;
                                }
                            }
                        }


                    AA_001:

                        var posData = __instance._transmitBuffer[k];
                        var rotation = posData.rotation;
                        var pos = posData.position;

                        Server.Get.Events.Server.InvokeTransmitPlayerDataEvent(player, playerToShow, ref pos, ref rotation, ref showinvoid);

                        if (showinvoid)
                            __instance._transmitBuffer[k] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, playerToShow.PlayerId);
                        else
                            __instance._transmitBuffer[k] = new PlayerPositionData(pos, rotation, playerToShow.PlayerId);
                    }


                    var conn = player.Connection;
                    if (__instance._usedData <= 20)
                        conn.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
                    else
                    {
                        byte b = 0;
                        while ((int)b < __instance._usedData / 20)
                        {
                            conn.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, 20, b), 1);
                            b += 1;
                        }

                        byte b2 = (byte)(__instance._usedData % (int)(b * 20));

                        if (b2 > 0)
                            conn.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, b2, b), 1);
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

    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.LookFor173))]
    internal static class PeanutPatch
    {
        private static void Postfix(ref bool __result, Scp173PlayerScript __instance, GameObject scp)
        {
            var player = __instance.GetPlayer();
            var peanut = scp.GetPlayer();
            if (!__result) return;

            if (player.Invisible || (player.RealTeam == Team.SCP && !Server.Get.Configs.synapseConfiguration.ScpTrigger173) || Server.Get.Configs.synapseConfiguration.CantLookAt173.Contains(player.RoleID))
                __result = false;

            if (peanut.Scp173Controller.IgnoredPlayers.Contains(player) || player.Scp173Controller.TurnedPlayers.Contains(player))
                __result = false;
        }
    }
}
