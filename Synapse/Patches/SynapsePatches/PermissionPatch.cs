using System;
using Harmony;
using MEC;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(ServerRoles),nameof(ServerRoles.SetGroup))]
    internal static class PermissionPatch
    {
        private static void Postfix(ServerRoles __instance)
        {
            try
            {
                Player player = __instance.GetPlayer();
                //Permission Stuff
                var group = player.SynapseGroup;
                if (player.ServerRoles.Group == null)
                    Logger.Get.Info("Group null");
                player.ServerRoles.Group.Permissions = group.GetVanillaPermissionValue();
                player.ServerRoles.Permissions = group.GetVanillaPermissionValue();

                player.ServerRoles.Group.Cover = group.Cover;

                player.ServerRoles.Group.RequiredKickPower = group.RequiredKickPower;
                player.ServerRoles.Group.KickPower = group.KickPower;


                player.RankName = group.Badge.ToUpper() == "NONE" ? null : group.Badge;
                player.RankColor = group.Color.ToUpper() == "NONE" ? null : group.Color;

                player.ServerRoles.Group.HiddenByDefault = group.Hidden;
                if (group.Hidden)
                    player.HideRank = true;

                if (group.RemoteAdmin)
                    player.RaLogin();
                else
                    player.RaLogout();
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Permission: PlayerSetGroup failed!!\n{e}");
            }
        }
    }
}
