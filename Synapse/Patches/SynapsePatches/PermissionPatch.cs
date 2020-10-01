using System;
using Harmony;
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
                Refresh(__instance.GetPlayer()) ;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Permission: PlayerSetGroup failed!!\n{e}");
            }
        }

        private static void Refresh(Player player)
        {
            var group = player.SynapseGroup;

            if(player.ServerRoles.Group == null)
            {
                var vgroup = new UserGroup()
                {
                    Shared = false,
                    BadgeColor = null,
                    BadgeText = null,
                    Cover = false,
                    HiddenByDefault = false,
                    KickPower = byte.MinValue,
                    Permissions = byte.MinValue,
                    RequiredKickPower = byte.MinValue,
                };
                player.ServerRoles.SetGroup(vgroup,false);
                return;
            }

            if (Server.Get.PermissionHandler.ServerSection.GlobalAccess && player.ServerRoles.RemoteAdminMode == ServerRoles.AccessMode.GlobalAccess)
                return;

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
    }

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshPermissions))]
    internal static class PermissionPatch2
    {
        private static bool Prefix(ServerRoles __instance)
        {
            if (__instance.GetPlayer().Rank == null)
                __instance.SetGroup(null, false);
            return false;
        }
    }
}
