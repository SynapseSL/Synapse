using System;
using Harmony;
using NorthwoodLib;
using Synapse.Api;

namespace Synapse.Patches.SynapsePatches.Permission_System
{
    internal class SignatureCompletePatch
    {
        private void Postfix(ServerRoles __instance, string challenge, string response, string publickey, bool hide)
        {
            try
            {
                var player = __instance.GetPlayer();

                if (challenge.StartsWith("badge-server-") && challenge == __instance._badgeChallenge)
                {
                    var dictionary = CentralAuth.ValidatePartialAuthToken(__instance._globalBadgeUnconfirmed, player.ClassManager.SaltedUserId, player.NickName, player.ClassManager.AuthTokenSerial, "Badge request");
                    player.globalBadgeRequest = dictionary;


                    if (dictionary == null)
                        return;

                    var flag = false;

                    if (dictionary["Badge type"] == "0")
                    {
                        player.GlobalBadge = Api.Enum.GlobalBadge.Patreon;
                        flag = true;
                    }

                    if (dictionary["Staff"] == "YES")
                    {
                        player.GlobalBadge = Api.Enum.GlobalBadge.Staff;
                        flag = true;
                    }

                    if (dictionary["Global banning"] == "YES")
                    {
                        player.GlobalBadge = Api.Enum.GlobalBadge.GlobalBanning;
                        flag = true;
                    }

                    if (dictionary["Management"] == "YES")
                    {
                        player.GlobalBadge = Api.Enum.GlobalBadge.Manager;
                        flag = true;
                    }

                    if (!flag)
                        player.GlobalBadge = Api.Enum.GlobalBadge.None;
                }
            }
            catch(Exception e)
            {
                Logger.Get.Info($"Synapse-Permission: ServerSignature Postfix failed!!\n{e}");
            }
        }
    }
}
