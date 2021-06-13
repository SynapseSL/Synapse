using HarmonyLib;
using MEC;
using Mirror;
using Synapse.Api;
using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp173
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.FixedUpdate))]
    internal static class Scp173BlinkingPatch
    {
        private static bool Prefix(Scp173PlayerScript __instance)
        {
            try
            {
                if ((Scp173PlayerScript._remainingTime - Time.fixedDeltaTime) >= 0f)
                    return true;
                if (!SynapseController.Server.Map.Round.RoundIsActive)
                    return true;

                __instance.DoBlinkingSequence();
                if (!__instance.iAm173 || (!__instance.isLocalPlayer && !NetworkServer.active))
                {
                    return false;
                }

                HashSet<Player> players = new();

                //Get all players looking at Scp173
                foreach (GameObject gameObject in PlayerManager.players)
                {
                    Scp173PlayerScript component = gameObject.GetComponent<Scp173PlayerScript>();
                    if (!component.SameClass && component.LookFor173(__instance.gameObject, true) && __instance.LookFor173(component.gameObject, false))
                    {
                        players.Add(gameObject.GetPlayer());
                    }
                }

                var scriptHolder = __instance.gameObject.GetPlayer();
                scriptHolder.Scp173Controller.ConfrontingPlayers = players;

                //If someone is confronting Scp173 when blinking
                if (scriptHolder.Scp173Controller.ConfrontingPlayers.Count != 0)
                    Server.Get.Events.Scp.Scp173.InvokeScp173BlinkEvent(scriptHolder);

                __instance.AllowMove = true;

                //Original
                foreach (GameObject gameObject in PlayerManager.players)
                {
                    Scp173PlayerScript component = gameObject.GetComponent<Scp173PlayerScript>();
                    if (!component.SameClass && component.LookFor173(__instance.gameObject, true) && __instance.LookFor173(component.gameObject, false))
                    {
                        __instance.AllowMove = false;
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp173BlinkEvent(Scp173) failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
            return false;
        }
    }
}
