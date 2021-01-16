using CustomPlayerEffects;
using HarmonyLib;
using Mirror;
using PlayableScps;
using System;
using UnityEngine;

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
                __instance._usedData = players.Count;

                if (__instance._receivedData == null || __instance._receivedData.Length < __instance._usedData)
                    __instance._receivedData = new PlayerPositionData[__instance._usedData * 2];

                for (var i = 0; i < __instance._usedData; i++)
                    __instance._receivedData[i] = new PlayerPositionData(players[i].Hub);

                if (__instance._transmitBuffer == null || __instance._transmitBuffer.Length < __instance._usedData)
                    __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];

                foreach (var player in players)
                {
                    Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);
                    if (player.RoleID == (int)RoleType.Scp93953 || player.RoleID == (int)RoleType.Scp93989)
                    {
                        for (var j = 0; j < __instance._usedData; j++)
                            if (__instance._transmitBuffer[j].position.y < 800f)
                            {
                                var newplayer = players[j];
                                if (newplayer.RealTeam != Team.SCP && newplayer.RealTeam != Team.RIP && !newplayer.GetComponent<Scp939_VisionController>().CanSee(player.ClassManager.Scp939))
                                    __instance._transmitBuffer[j] = new PlayerPositionData(Vector3.up * 6000f, 0f, __instance._transmitBuffer[j].playerID);
                            }
                    }
                    else
                    {
                        for (int k = 0; k < __instance._usedData; k++)
                        {
                            var showinvoid = false;
                            var newplayer = players[k];
                            var vector = __instance._transmitBuffer[k].position - player.Position;

                            if (player.RoleType == RoleType.Scp173 && player.Scp173Controller.IgnoredPlayers.Contains(newplayer))
                            {
                                showinvoid = true;
                                goto AA_001;
                            }

                            if (newplayer.Invisible && !player.HasPermission("synapse.see.invisible"))
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

                                if (newplayer != null)
                                {
                                    var scp = player.Hub.scpsController.CurrentScp as Scp096;

                                    if (scp != null && scp.Enraged && !scp.HasTarget(newplayer.Hub) && newplayer.RealTeam != Team.SCP)
                                    {
                                        showinvoid = true;
                                        goto AA_001;
                                    }
                                    if (newplayer.Hub.playerEffectsController.GetEffect<Scp268>().Enabled)
                                    {
                                        var flag = false;
                                        if (scp != null)
                                            flag = scp.HasTarget(newplayer.Hub);

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
                            if (showinvoid)
                                __instance._transmitBuffer[k] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, newplayer.PlayerId);
                        }
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
                Api.Logger.Get.Error($"Synapse-InvisibleMode: TransmitData failed failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
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

            __result = player?.Invisible != true && __result;

            if (peanut.RoleType == RoleType.Scp173 && peanut.Scp173Controller.IgnoredPlayers.Contains(player))
                __result = false;
        }
    }
}
