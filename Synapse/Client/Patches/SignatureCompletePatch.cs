using System;
using System.Collections.Generic;
using HarmonyLib;
using MEC;
using Org.BouncyCastle.Utilities;
using RemoteAdmin;
using Swan;
using Synapse.Api;

namespace Synapse.Client.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.CallCmdServerSignatureComplete))]
    public static class SignatureCompletePatch
    {
        public static bool Prefix(ServerRoles __instance, string challenge, string response, string publickey, bool hide)
        {
            if (challenge.Equals("synapse-client-authentication") && ClientManager.IsSynapseClientEnabled)
            {
                try
                {
                    var payload = SynapseController.ClientManager.DecodeJWT(response);
                    Logger.Get.Warn($"{payload.Humanize()}, logging in");
                    __instance._ccm.UserId = payload.uuid;
                    __instance._ccm.SyncedUserId = payload.uuid;
                    __instance.PublicKeyAccepted = true;
                    __instance._hub.nicknameSync.UpdateNickname(payload.sub);
                    ServerConsole.NewPlayers.Add(__instance._ccm);
                    var sessionBytes = Utf8.GetBytes(payload.session);
                    if (sessionBytes.Length != 24)
                    {
                        Logger.Get.Info("Wrong Session Token Length?");
                        return true;
                    }
                    var paddedSessionToken = new byte[32];
                    var queryProcessor = __instance.GetComponent<QueryProcessor>();
                    for (int i = 0; i < 24; i++) paddedSessionToken[i] = sessionBytes[i];
                    for (int i = 24; i < 32; i++) paddedSessionToken[i] = 0x00;
                    queryProcessor.Key = paddedSessionToken;
                    queryProcessor.Salt = new byte[32];
                    Arrays.Fill(queryProcessor.Salt, 0x00);
                    queryProcessor.ClientSalt = queryProcessor.Salt;
                    queryProcessor._clientSalt = queryProcessor.ClientSalt;
                    queryProcessor._key = queryProcessor.Key;
                    queryProcessor.CryptoManager.EncryptionKey = queryProcessor.Key;
                    Logger.Get.Info("Updated Crypto Details");
                    __instance.RefreshPermissions(false); //Just since its done in base code
                    Timing.RunCoroutine(_RefreshPermissionLate(__instance.GetPlayer()));

                }
                catch (Exception e)
                {
                    Logger.Get.Error(e);
                }
                return false;
            }

            return true;
        }
        

        private static IEnumerator<float> _RefreshPermissionLate(Player player)
        {
            yield return Timing.WaitForOneFrame;
            player.RefreshPermission(true);
        }
    }
}