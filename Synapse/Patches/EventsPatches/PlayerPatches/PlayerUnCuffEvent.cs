using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(Handcuffs),nameof(Handcuffs.CallCmdUncuffTarget))]
    internal static class PlayerUnCuffEvent
    {
        private static bool Prefix(Handcuffs __instance)
        {
            try
            {
                var player = __instance.GetPlayer();
                var cuffed = Server.Get.Players.FirstOrDefault(x => x.Cuffer == player);

                if (player == null || cuffed == null) return false;

                Server.Get.Events.Player.InvokeUncuff(player, cuffed, true, out var allow);

                return allow;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerUnCuff event failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdFreeTeammate))]
    internal static class PlayerFreeTeamMate
    {
        private static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true)) return false;

                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
                    return false;

                if (__instance.MyReferenceHub.characterClassManager.CurRole.team == Team.SCP)
                    return false;

                var player = __instance.GetPlayer();
                var cuffed = target.GetPlayer();

                Server.Get.Events.Player.InvokeUncuff(player, cuffed, false, out var allow);

                if (allow)
                    cuffed.Cuffer = null;

                return false;
            }
            catch(Exception e)
            {
                SynapseController.Server.Logger.Error($"Synapse-Event: PlayerUnCuff(FreeTeamMate) event failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                return true;
            }
        }
    }
}
