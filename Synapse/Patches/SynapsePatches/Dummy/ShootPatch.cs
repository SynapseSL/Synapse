using Achievements;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.ShowHitIndicator))]
    internal static class ShowHitPatch
    {
        [HarmonyPrefix]
        private static bool HitInidcatorFix(uint netId, float damage, Vector3 origin)
        {
            if (!ReferenceHub.TryGetHubNetID(netId, out var referenceHub))
            {
                return false;
            }

            var player = referenceHub.GetPlayer();
            if (player is null || player.IsDummy)
            {
                return false;
            }

            foreach (var referenceHub2 in referenceHub.spectatorManager.ServerCurrentSpectatingPlayers)
            {
                referenceHub2.networkIdentity.connectionToClient.Send(new GunHitMessage(false, damage, origin));
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FirearmExtensions), nameof(FirearmExtensions.ServerSendAudioMessage))]
    internal static class SendAudioPatch
    {
        [HarmonyPrefix]
        private static bool FirearmPlaySound(this Firearm firearm, byte clipId)
        {
            var firearmAudioClip = firearm.AudioClips[clipId];
            var owner = firearm.Owner;

            var num = firearmAudioClip.HasFlag(FirearmAudioFlags.ScaleDistance) ? (firearmAudioClip.MaxDistance * firearm.AttachmentsValue(AttachmentParam.GunshotLoudnessMultiplier)) : firearmAudioClip.MaxDistance;
            if (firearmAudioClip.HasFlag(FirearmAudioFlags.IsGunshot) && owner.transform.position.y > 900f)
            {
                num *= 2.3f;
            }

            var soundReach = num * num;
            foreach (var referenceHub in ReferenceHub.GetAllHubs().Values)
            {
                var player = referenceHub.GetPlayer();
                if (player is null || player.IsDummy)
                {
                    return false;
                }

                if (referenceHub != firearm.Owner)
                {
                    var curClass = referenceHub.characterClassManager.CurClass;
                    if (curClass == RoleType.Spectator || curClass == RoleType.Scp079 || (referenceHub.transform.position - owner.transform.position).sqrMagnitude <= soundReach)
                    {
                        referenceHub.networkIdentity.connectionToClient.Send(new GunAudioMessage(owner, clipId, (byte)Mathf.RoundToInt(Mathf.Clamp(num, 0f, 255f)), referenceHub), 0);
                    }
                }
            }

            var serverSoundPlayed = FirearmExtensions.ServerSoundPlayed;
            if (serverSoundPlayed is null)
            {
                return false;
            }

            serverSoundPlayed.Invoke(firearm, clipId, num);
            return false;
        }
    }

    [HarmonyPatch(typeof(AchievementHandlerBase), nameof(AchievementHandlerBase.ServerAchieve))]
    internal static class AchievePatch
    {
        [HarmonyPrefix]
        private static bool OnAchieve(NetworkConnection conn) => conn != null;
    }
}
