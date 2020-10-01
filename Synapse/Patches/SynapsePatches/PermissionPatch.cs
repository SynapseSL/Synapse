using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptography;
using Harmony;
using RemoteAdmin;
using Synapse.Api;
using UnityEngine.Assertions.Must;

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
            var group = Server.Get.PermissionHandler.GetPlayerGroup(player);

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

            player.SynapseGroup = group;
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
