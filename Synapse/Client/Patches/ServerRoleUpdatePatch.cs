using System;
using Synapse.Api;
using HarmonyLib;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(ServerRoles),nameof(ServerRoles.Update))]
    internal static class ServerRoleUpdatePatch
    {
        private static bool Prefix(ServerRoles __instance)
        {
            if (!ClientManager.IsSynapseClientEnabled) return true;
            try
            {
                var player = __instance.GetPlayer();

                if (__instance.CurrentColor == null) return false;

                //I think this is only ever executed at the client since FixedBadge should be always null at the server but it can't be wrong to add it to be sure
                if (!string.IsNullOrEmpty(__instance.FixedBadge) && __instance.MyText != __instance.FixedBadge)
                {
                    __instance.SetText(__instance.FixedBadge);
                    __instance.SetColor("silver");
                    return false;
                }
                if(!string.IsNullOrEmpty(__instance.FixedBadge) && __instance.CurrentColor.Name != "silver")
                {
                    __instance.SetColor("silver");
                    return false;
                }

                if (__instance.GlobalBadge != __instance._prevBadge)
                {
                    __instance._prevBadge = __instance.GlobalBadge;
                    if (player.GlobalSynapseGroup == null)
                    {
                        __instance._bgc = null;
                        __instance._bgt = null;
                        __instance._authorizeBadge = false;
                        __instance._prevColor += ".";
                        __instance._prevText += ".";
                        return false;
                    }

                    if (player.GlobalSynapseGroup.Color == "(none)" || player.GlobalSynapseGroup.Name == "(none)")
                    {
                        __instance._bgc = null;
                        __instance._bgt = null;
                        __instance._authorizeBadge = false;
                    }
                    else
                    {
                        __instance.NetworkMyText = player.GlobalSynapseGroup.Name;
                        __instance._bgt = player.GlobalSynapseGroup.Name;

                        __instance.NetworkMyColor = player.GlobalSynapseGroup.Color;
                        __instance._bgc = player.GlobalSynapseGroup.Color;

                        __instance._authorizeBadge = true;
                    }
                }

                if (__instance._prevColor == __instance.MyColor && __instance._prevText == __instance.MyText) return false;

                if (__instance.CurrentColor.Restricted && (__instance.MyText != __instance._bgt || __instance.MyColor != __instance._bgc))
                {
                    GameCore.Console.AddLog($"TAG FAIL 1 - {__instance.MyText} - {__instance._bgt} /-/ {__instance.MyColor} - {__instance._bgc}", UnityEngine.Color.gray, false);
                    __instance._authorizeBadge = false;
                    __instance.NetworkMyColor = "default";
                    __instance.NetworkMyText = null;
                    __instance._prevColor = "default";
                    __instance._prevText = null;
                    PlayerList.UpdatePlayerRole(__instance.gameObject);
                    return false;
                }

                if (__instance.MyText != null && __instance.MyText != __instance._bgt && (__instance.MyText.Contains("[") || __instance.MyText.Contains("]") || __instance.MyText.Contains("<") || __instance.MyText.Contains(">")))
                {
                    GameCore.Console.AddLog($"TAG FAIL 2 - {__instance.MyText} - {__instance._bgt} /-/ {__instance.MyColor} - {__instance._bgc}", UnityEngine.Color.gray, false);
                    __instance._authorizeBadge = false;
                    __instance.NetworkMyColor = "default";
                    __instance.NetworkMyText = null;
                    __instance._prevColor = "default";
                    __instance._prevText = null;
                    PlayerList.UpdatePlayerRole(__instance.gameObject);
                    return false;
                }

                __instance._prevColor = __instance.MyColor;
                __instance._prevText = __instance.MyText;
                __instance._prevBadge = __instance.GlobalBadge;
                PlayerList.UpdatePlayerRole(__instance.gameObject);

                return false;
            }
            catch(Exception e)
            {
                Logger.Get.Error($"Synapse-Client: Update ServerRole Patch failed:\n{e}");
                return true;
            }
        }
    }
}
