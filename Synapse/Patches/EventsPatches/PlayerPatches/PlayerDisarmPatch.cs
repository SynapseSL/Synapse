using System;
using UnityEngine;
using HarmonyLib;
using GameCore;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Handcuffs),nameof(Handcuffs.CallCmdCuffTarget))]
    internal static class PlayerDisarmPatch
    {
        private static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute()) return false;

                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) >
                    __instance.raycastDistance * 1.1f) return false;

                var targetplayer = target.GetPlayer();
                var player = __instance.GetPlayer();
                var item = player.ItemInHand;

                var handcuffs = targetplayer.Handcuffs;
                if (item.ItemType != ItemType.Disarmer) return false;

                if (handcuffs.CufferId >= 0 || __instance.ForceCuff || handcuffs.MyReferenceHub.inventory.curItem != ItemType.None)
                    return false;


                //Team of the person who cuffs someone
                var team = __instance.MyReferenceHub.characterClassManager.CurRole.team;
                //Team of the Person who will become cuffed
                var team2 = handcuffs.MyReferenceHub.characterClassManager.CurRole.team;

                var flag = false;

                switch (team)
                {
                    //Check for When the Cuffer is a DBoy
                    case Team.CDP:
                        {
                            if (team2 == Team.MTF || team2 == Team.RSC) flag = true;
                            break;
                        }
                    //Check for when the Cuffer is a Nerd
                    case Team.RSC:
                        {
                            if (team2 == Team.CHI || team2 == Team.CDP) flag = true;
                            break;
                        }
                    //Check for when the Cuffer is a Chaos
                    case Team.CHI:
                        {
                            switch (team2)
                            {
                                case Team.MTF:
                                case Team.RSC:
                                case Team.CDP when ConfigFile.ServerConfig.GetBool("ci_can_cuff_class_d"):
                                    flag = true;
                                    break;
                            }

                            break;
                        }
                    //Check for when the Cuffer is a Mtf
                    case Team.MTF:
                        {
                            switch (team2)
                            {
                                case Team.CHI:
                                case Team.CDP:
                                case Team.RSC when ConfigFile.ServerConfig.GetBool("mtf_can_cuff_researchers"):
                                    flag = true;
                                    break;
                            }

                            break;
                        }
                }

                //Event
                var cuffer = __instance.GetPlayer();
                var target2 = handcuffs.GetPlayer();
                SynapseController.Server.Events.Player.InvokePlayerCuffTargetEvent(target2, cuffer, item, ref flag);
                SynapseController.Server.Events.Player.InvokePlayerItemUseEvent(cuffer, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref flag);

                if (!flag) return false;

                if (team2 == global::Team.MTF && team == global::Team.CDP)
                {
                    __instance.MyReferenceHub.playerStats.TargetAchieve(__instance.MyReferenceHub.playerStats.connectionToClient, "tableshaveturned");
                }

                __instance.ClearTarget();
                handcuffs.NetworkCufferId = __instance.MyReferenceHub.queryProcessor.PlayerId;

                return false;
            }
            catch (Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: PlayerAmmoDrop failed!!\n{e}");
                return true;
            }
        }
    }
}
