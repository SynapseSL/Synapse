using System;
using HarmonyLib;

namespace Synapse.Patches.EventsPatches.ScpPatches.Scp173
{
    internal static class Scp173AttackPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayableScps.Scp173), nameof(PlayableScps.Scp173.ServerKillPlayer))]
        private static bool ServerKillPlayerPatch(PlayableScps.Scp173 __instance, ReferenceHub target)
        {
            try
            {
                var scp = __instance.GetPlayer();
                var player = target.GetPlayer();

                if (target == __instance.Hub || player.ClassManager.IsAnyScp() || player.ClassManager.CurClass == RoleType.Spectator)
                    return false;

                if (!HitboxIdentity.CheckFriendlyFire(scp.Hub, player.Hub))
                    return false;

                SynapseController.Server.Events.Scp.InvokeScpAttack(scp, player, Api.Enum.ScpAttackType.Scp173_Snap, out var allow);

                return allow;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-Event: ScpAttackEvent(Scp173) failed!!\n{e}");
                return true;
            }
        }
    }
}
