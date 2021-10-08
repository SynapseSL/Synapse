using System;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.Dummy
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.ShowHitIndicator))]
    internal static class ShowHitPatch
    {
        [HarmonyPrefix]
        private static bool HitInidcatorFix(uint netId, float damage, Vector3 origin)
        {
            if (!ReferenceHub.TryGetHubNetID(netId, out ReferenceHub referenceHub))
            {
                return false;
            }
            var player = referenceHub.GetPlayer();
            if (player == null || player.IsDummy)
            {
                return false;
            }
            foreach (ReferenceHub referenceHub2 in referenceHub.spectatorManager.ServerCurrentSpectatingPlayers)
            {
                referenceHub2.networkIdentity.connectionToClient.Send(new GunHitMessage
                {
                    Weapon = ItemType.None,
                    Damage = (byte)Mathf.Round(damage * 2.5f),
                    DamagePosition = origin
                }, 0);
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
            FirearmAudioClip firearmAudioClip = firearm.AudioClips[clipId];
            ReferenceHub owner = firearm.Owner;

            float num = firearmAudioClip.HasFlag(FirearmAudioFlags.ScaleDistance) ? (firearmAudioClip.MaxDistance * firearm.AttachmentsValue(AttachmentParam.GunshotLoudnessMultiplier)) : firearmAudioClip.MaxDistance;
            if (firearmAudioClip.HasFlag(FirearmAudioFlags.IsGunshot) && owner.transform.position.y > 900f)
            {
                num *= 2.3f;
            }

            float soundReach = num * num;
            foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
            {
                var player = referenceHub.GetPlayer();
                if (player == null || player.IsDummy)
                {
                    return false;
                }
                if (referenceHub != firearm.Owner)
                {
                    RoleType curClass = referenceHub.characterClassManager.CurClass;
                    if (curClass == RoleType.Spectator || curClass == RoleType.Scp079 || (referenceHub.transform.position - owner.transform.position).sqrMagnitude <= soundReach)
                    {
                        referenceHub.networkIdentity.connectionToClient.Send(new GunAudioMessage(owner, clipId, (byte)Mathf.RoundToInt(Mathf.Clamp(num, 0f, 255f)), referenceHub), 0);
                    }
                }
            }

            Action<Firearm, byte, float> serverSoundPlayed = FirearmExtensions.ServerSoundPlayed;
            if (serverSoundPlayed == null)
            {
                return false;
            }

            serverSoundPlayed.Invoke(firearm, clipId, num);
            return false;
        }
    }
}
