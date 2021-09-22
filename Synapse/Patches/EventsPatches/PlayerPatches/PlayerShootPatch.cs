using System;
using System.Linq;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Patches.EventsPatches.PlayerPatches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.ShowHitIndicator))]
    internal static class PlayerShootPatch1
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
    internal static class PlayerShootPatch2
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

    [HarmonyPatch(typeof(FirearmBasicMessagesHandler), nameof(FirearmBasicMessagesHandler.ServerShotReceived))]
    internal static class PlayerShootPatch3
    {
        [HarmonyPrefix]
        private static bool ServerProcessShotPatch(NetworkConnection conn, ShotMessage msg)
        {
            try
            {
                var player = conn.GetPlayer();

                if (!player.VanillaInventory.UserInventory.Items.TryGetValue(msg.ShooterWeaponSerial, out var itembase))
                    return false;
                var item = itembase.GetSynapseItem();

                Synapse.Api.Player target;
                if (msg.TargetNetId != 0)
                {
                    target = Server.Get.Players.FirstOrDefault(x => x.NetworkIdentity.netId == msg.TargetNetId);
                    if (target == null)
                    {
                        target = Server.Get.Map.Dummies.FirstOrDefault(x => x.Player.NetworkIdentity?.netId == msg.TargetNetId)?.Player;
                    }
                }
                else
                {
                    target = null;
                }

                Server.Get.Events.Player.InvokePlayerShootEvent(player, target, msg.TargetPosition, item, out var allow);
                Server.Get.Events.Player.InvokePlayerItemUseEvent(player, item, Api.Events.SynapseEventArguments.ItemInteractState.Finalizing, ref allow);

                if (allow)
                {
                    if (!ReferenceHub.TryGetHub(conn.identity.gameObject, out ReferenceHub referenceHub))
                    {
                        return false;
                    }
                    if (msg.ShooterWeaponSerial != referenceHub.inventory.CurItem.SerialNumber)
                    {
                        return false;
                    }
                    if (referenceHub.inventory.CurInstance is Firearm firearm && firearm.ActionModule.ServerAuthorizeShot())
                    {
                        firearm.HitregModule.ServerProcessShot(msg);
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Get.Error($"Synapse-Event: PlayerShoot failed!!\n{e}");
                return true;
            }
        }
    }
}
