using System;
using System.Linq;
using HarmonyLib;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.GetShootPermission), new Type[] { typeof(CharacterClassManager), typeof(bool) })]
    internal static class ShootPermissionPatch
    {
        private static bool Prefix(WeaponManager __instance,out bool __result, CharacterClassManager c, bool forceFriendlyFire = false)
        {
            try
            {
                var shooter = __instance.GetPlayer();
                var target = c.GetPlayer();

                __result = true;

                if (shooter.CustomRole == null && target.CustomRole == null)
                {
                    if (shooter.Team == Team.SCP && target.Team == Team.SCP) __result = false;

                    var ff = Server.Get.FF;
                    if (forceFriendlyFire)
                        ff = true;

                    else if (!ff) __result = Misc.GetFraction(shooter.Team) != Misc.GetFraction(target.Team);
                }
                else
                {
                    if (shooter.CustomRole != null)
                    {
                        if (shooter.CustomRole.GetFriends().Any(x => x == target.RealTeam))
                        {
                            __result = false;
                            shooter.GiveTextHint(Server.Get.Configs.synapseTranslation.ActiveTranslation.sameTeam);
                        }
                    }
                    if (target.CustomRole != null)
                    {
                        if (target.CustomRole.GetFriends().Any(x => x == shooter.RealTeam))
                        {
                            __result = false;
                            shooter.GiveTextHint(Server.Get.Configs.synapseTranslation.ActiveTranslation.sameTeam);
                        }
                    }
                }
                return false;
            }
            catch(Exception e)
            {
                Synapse.Api.Logger.Get.Error($"Synapse-API: GetShootPermission  failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
                __result = true;
                return true;
            }
        }
    }
}
