using System;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches.PermissionSystem
{
    internal class SignatureCompletePatch
    {
        private void Postfix(ServerRoles __instance, string challenge)
        {
            try
            {
                var player = __instance.GetPlayer();

                if (challenge.StartsWith("badge-server-") && challenge == __instance._badgeChallenge)
                {
                    var dictionary = CentralAuth.ValidatePartialAuthToken(__instance._globalBadgeUnconfirmed, player.ClassManager.SaltedUserId, player.NickName, player.ClassManager.AuthTokenSerial, "Badge request");


                    if (dictionary == null)
                        return;

                    player.GlobalBadge = Api.Enum.GlobalBadge.None;

                    if (dictionary["Badge type"] == "0")
                        player.GlobalBadge = Api.Enum.GlobalBadge.Patreon;

                    if (dictionary["Staff"] == "YES")
                        player.GlobalBadge = Api.Enum.GlobalBadge.Staff;

                    if (dictionary["Global banning"] == "YES")
                        player.GlobalBadge = Api.Enum.GlobalBadge.GlobalBanning;

                    if (dictionary["Management"] == "YES")
                        player.GlobalBadge = Api.Enum.GlobalBadge.Manager;
                }
            }
            catch(Exception e)
            {
                Logger.Get.Info($"Synapse-Permission: ServerSignature Postfix failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
            }
        }
    }
}
