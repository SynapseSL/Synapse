using CommandSystem;
using HarmonyLib;
using Hazards;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.Ragdolls;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using PlayerStatsSystem;
using Respawning;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Player;
using Synapse3.SynapseModule.Role;
using System;
using System.Linq;
using InventorySystem.Searching;
using MapGeneration.Distributors;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Visibility;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS
[Automatic]
[SynapsePatch("FallingIntoAbyss", PatchType.PlayerEvent)]
public static class FallingIntoAbyssPatch
{
    private static readonly PlayerEvents _playerEvents;
    static FallingIntoAbyssPatch() => _playerEvents = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheckpointKiller), nameof(CheckpointKiller.OnTriggerEnter))]
    public static bool FallingIntoAbyss(Collider other)
    {
        try
        {
            OnFallingIntoAbyss(other);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("FallingIntoAbyss Patch failed\n" + ex);
            return true;
        }
    }

    private static void OnFallingIntoAbyss(Collider other)
    {
        var player = other?.GetComponentInParent<SynapsePlayer>();
        if (player == null) return;
        var ev = new FallingIntoAbyssEvent(player, true);
        _playerEvents.FallingIntoAbyss.RaiseSafely(ev);
        if (!ev.Allow) return;

        player.Hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Crushed));
    }
}

//TODO: Make this to a replaced handler instead of a patch
[Automatic]
[SynapsePatch("Speak", PatchType.PlayerEvent)]
public static class SpeakPatch
{
    private const int ProximityRange = 100; //Take the root of this so the range is 10

    private static readonly PlayerEvents Player;
    private static readonly SynapseConfigService Config;

    static SpeakPatch()
    {
        Player = Synapse.Get<PlayerEvents>();
        Config = Synapse.Get<SynapseConfigService>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static bool OnServerReceiveMessage(NetworkConnection conn, VoiceMessage msg)
    {
        try
        {
            if (msg.SpeakerNull || msg.Speaker.netId != conn.identity.netId) return false;
            var player = msg.Speaker.GetSynapsePlayer();

            if (player.CurrentRole is not IVoiceRole voiceRoleSpeaker
                || !voiceRoleSpeaker.VoiceModule.CheckRateLimit()) return false;

            var flags = VoiceChatMutes.GetFlags(msg.Speaker);
            if (flags is VcMuteFlags.GlobalRegular or VcMuteFlags.LocalRegular) return false;

            var voiceChatChannel = voiceRoleSpeaker.VoiceModule.ValidateSend(msg.Channel);
            var ev = new SpeakEvent(player, true, voiceChatChannel, msg.Data, msg.DataLength);
            Player.Speak.RaiseSafely(ev);
            if (ev.Channel == VoiceChatChannel.None || !ev.Allow) return false;
            voiceRoleSpeaker.VoiceModule.CurrentChannel = ev.Channel;
            var checkForScpProximity = player.Team == Team.SCPs && player.MainScpController.ProximityChat;

            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub == player.Hub) continue;
                if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole) continue;
                var receiver = hub.GetSynapsePlayer();
                var validatedChannel = voiceRole.VoiceModule.ValidateReceive(msg.Speaker, ev.Channel);
                var isSpectator = receiver.RoleType is RoleTypeId.Spectator or RoleTypeId.Overwatch;

                if (Config.GamePlayConfiguration.SpectatorListenOnSCPs && isSpectator && player.Team == Team.SCPs)
                {
                    if (receiver.CurrentlySpectating?.Team == Team.SCPs)
                        validatedChannel = VoiceChatChannel.Proximity;
                }

                if (checkForScpProximity)
                {
                    if (isSpectator)
                    {
                        var spectating = receiver.CurrentlySpectating;
                        if (receiver.CurrentlySpectating == player)
                            validatedChannel = VoiceChatChannel.Proximity;
                        else if (spectating != null &&
                                 (spectating.Position - player.Position).sqrMagnitude < ProximityRange)
                            validatedChannel = VoiceChatChannel.Proximity;
                    }
                    else if ((receiver.Position - player.Position).sqrMagnitude < ProximityRange)
                        validatedChannel = VoiceChatChannel.Proximity;
                    else
                        validatedChannel = VoiceChatChannel.None;
                }

                var ev2 = new SpeakToPlayerEvent(player, receiver, true, validatedChannel, msg.Data, msg.DataLength);
                Player.SpeakToPlayer.RaiseSafely(ev2);
                if (ev2.Channel == VoiceChatChannel.None || !ev2.Allow) continue;
                msg.Channel = ev2.Channel;
                hub.connectionToClient.Send(msg);
            }
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Speak Patch Failed:\n" + ex);
        }

        return false;
    }
}

[Automatic]
[SynapsePatch("PlayerDoorInteract", PatchType.PlayerEvent)]
public static class DoorInteractPatch
{
    private static readonly PlayerEvents PlayerEvents;
    static DoorInteractPatch() => PlayerEvents = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DoorVariant), nameof(DoorVariant.ServerInteract))]
    public static bool OnDoorInteract(DoorVariant __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            var player = ply.GetSynapsePlayer();
            var allow = false;
            var bypassDenied = false;
            var door = __instance;
            if (__instance.ActiveLocks > 0 && !ply.serverRoles.BypassMode)
            {
                var mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);

                var canInteractGeneral =
                    mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.CanOpen);
                var scpOverride = mode.HasFlagFast(DoorLockMode.ScpOverride) && player.Team == Team.SCPs;
                var canChangeCurrently = mode != DoorLockMode.FullLock
                                         && (__instance.TargetState && mode.HasFlagFast(DoorLockMode.CanClose)
                                             || !__instance.TargetState && mode.HasFlagFast(DoorLockMode.CanOpen));

                if (!canInteractGeneral && !scpOverride && !canChangeCurrently)
                {
                    bypassDenied = true;
                }
            }

            //This is false when the Animation is still playing
            if (!door.AllowInteracting(player, colliderId)) return false;

            if (!bypassDenied)
            {
                if (player.RoleType == RoleTypeId.Scp079
                    || door.RequiredPermissions.CheckPermission(player))
                {
                    allow = true;
                }
            }

            var nwAllow = EventManager.ExecuteEvent(ServerEventType.PlayerInteractDoor, ply, __instance, allow);
            var ev = new DoorInteractEvent(player, allow && nwAllow, door.GetSynapseDoor(), bypassDenied);
            PlayerEvents.DoorInteract.RaiseSafely(ev);

            if (ev.Allow)
            {
                door.NetworkTargetState = !door.TargetState;
                door._triggerPlayer = player.Hub;
                return false;
            }

            if (ev.LockBypassRejected)
                door.LockBypassDenied(player, colliderId);

            if (ev.PlayDeniedSound)
                door.PermissionsDenied(player, colliderId);

            DoorEvents.TriggerAction(door, DoorAction.AccessDenied, player);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error($"DoorInteract Patch failed!!\n{ex}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerEscape", PatchType.PlayerEvent)]
public static class PlayerEscapeFacilityPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Escape), nameof(Escape.ServerHandlePlayer))]
    public static bool OnEscape(ReferenceHub hub)
    {
        try
        {
            var player = hub.GetSynapsePlayer();
            if (player == null) return false;
            player.TriggerEscape(false);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error($"PlayerEscape Patch failed!!\n{ex}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerNameChange", PatchType.PlayerEvent)]
public static class PlayerNameChangePatch
{
    private static readonly PlayerEvents Player;
    static PlayerNameChangePatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.DisplayName), MethodType.Setter)]
    public static void SetDisplayName(NicknameSync __instance, ref string value)

    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null) return;
            var ev = new UpdateDisplayNameEvent(player, value);
            Player.UpdateDisplayName.RaiseSafely(ev);
            value = ev.NewDisplayName;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Player Name Change Patch failed!!\n{e}");
        }
    }
}

[Automatic]
[SynapsePatch("PlayerHeal", PatchType.PlayerEvent)]
public static class PlayerHealPatch
{
    private static readonly PlayerEvents Player;
    static PlayerHealPatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.ServerHeal))]
    public static bool OnServerHeal(HealthStat __instance, ref float healAmount)
    {
        try
        {
            var player = __instance.Hub.GetSynapsePlayer();
            if (player == null) return true;

            var ev = new HealEvent(player, true, healAmount);
            Player.Heal.RaiseSafely(ev);
            healAmount = ev.Amount;
            return ev.Allow;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Player Heal Patch failed\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerOpenWarheadButton", PatchType.PlayerEvent)]
public static class PlayerOpenWarHeadButtonPatch
{
    private static readonly SynapseConfigService Config;
    private static readonly PlayerEvents Player;

    static PlayerOpenWarHeadButtonPatch()
    {
        Config = Synapse.Get<SynapseConfigService>();
        Player = Synapse.Get<PlayerEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdSwitchAWButton))]
    public static bool OnWarheadButton(PlayerInteract __instance)
    {
        try
        {
            if (!__instance.CanInteract) return false;
            var gameObject = GameObject.Find("OutsitePanelScript");
            if (!__instance.ChckDis(gameObject.transform.position)) return false;

            var player = __instance.GetSynapsePlayer();

            var componentInParent = gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>();
            if (componentInParent == null)
                return false;
            if (componentInParent.keycardEntered && !Config.GamePlayConfiguration.WarheadButtonClosable)
                return false;

            var allow = KeycardPermissions.AlphaWarhead.CheckPermission(player);

            var ev = new OpenWarheadButtonEvent(player, allow, !componentInParent.NetworkkeycardEntered);
            Player.OpenWarheadButton.RaiseSafely(ev);
            if (!ev.Allow) return false;

            __instance.OnInteract();
            componentInParent.NetworkkeycardEntered = ev.OpenButton;
            if (__instance._hub.TryGetAssignedSpawnableTeam(out var team))
                RespawnTokensManager.GrantTokens(team, 1f);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Player open Warhead button Patch failed\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerWarheadInteract", PatchType.PlayerEvent)]
public static class WarheadInteractPatch
{
    private static readonly PlayerEvents Player;
    private static readonly NukeService Nuke;

    static WarheadInteractPatch()
    {
        Player = Synapse.Get<PlayerEvents>();
        Nuke = Synapse.Get<NukeService>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdUsePanel))]
    public static bool OnPanelInteract(PlayerInteract __instance, PlayerInteract.AlphaPanelOperations n)
    {
        try
        {
            if (!__instance.CanInteract) return false;

            var player = __instance.GetSynapsePlayer();
            var nukeSide = AlphaWarheadOutsitePanel.nukeside;
            if (!__instance.ChckDis(nukeSide.transform.position))
                return false;

            var ev = new WarheadPanelInteractEvent(player, !Nuke.InsidePanel.Locked, n);
            Player.WarheadPanelInteract.RaiseSafely(ev);
            if (!ev.Allow) return false;

            __instance.OnInteract();
            switch (ev.Operation)
            {
                case PlayerInteract.AlphaPanelOperations.Cancel:
                    AlphaWarheadController.Singleton.CancelDetonation(__instance._hub);
                    ServerLogs.AddLog(ServerLogs.Modules.Warhead,
                        player.Hub.LoggedNameFromRefHub() + " cancelled the Alpha Warhead detonation.",
                        ServerLogs.ServerLogType.GameEvent);
                    return false;

                case PlayerInteract.AlphaPanelOperations.Lever:
                    if (!nukeSide.AllowChangeLevelState()) return false;
                    nukeSide.Networkenabled = !nukeSide.enabled;
                    __instance.RpcLeverSound();
                    ServerLogs.AddLog(ServerLogs.Modules.Warhead,
                        player.Hub.LoggedNameFromRefHub() + " set the Alpha Warhead status to " + nukeSide.enabled +
                        ".", ServerLogs.ServerLogType.GameEvent);
                    return false;
            }

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player WarheadInteract Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerTantrumHazard", PatchType.PlayerEvent)]
public static class PlayerTantrumHazardPatch
{
    private static readonly PlayerEvents Player;
    static PlayerTantrumHazardPatch() => Player = Synapse.Get<PlayerEvents>();


    [HarmonyPrefix]
    [HarmonyPatch(typeof(TantrumEnvironmentalHazard), nameof(TantrumEnvironmentalHazard.OnEnter))]
    public static bool OnEnterTantrum(TantrumEnvironmentalHazard __instance, ReferenceHub player)
    {
        try
        {
            if (!__instance.IsActive) return false;

            var sPlayer = player.GetSynapsePlayer();
            var allow = !(!Synapse3Extensions.CanHarmScp(sPlayer, false) || sPlayer.GodMode);

            var ev = new WalkOnTantrumEvent(sPlayer, allow, __instance);
            Player.WalkOnTantrum.RaiseSafely(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("PlayerTantrumHazard Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerSinkHoleHazard", PatchType.PlayerEvent)]
public static class PlayerSinkHoleHazardPatch
{
    private static readonly PlayerEvents Player;
    static PlayerSinkHoleHazardPatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SinkholeEnvironmentalHazard), nameof(SinkholeEnvironmentalHazard.OnEnter))]
    public static bool OnEnterSinkhole(SinkholeEnvironmentalHazard __instance, ReferenceHub player)
    {
        try
        {
            if (!__instance.IsActive) return false;

            var sPlayer = player.GetSynapsePlayer();
            var allow = !(!Synapse3Extensions.CanHarmScp(sPlayer, false) || sPlayer.GodMode);

            var ev = new WalkOnSinkholeEvent(sPlayer, allow, __instance);
            Player.WalkOnSinkhole.RaiseSafely(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("PlayerSinkhole Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerWorkstation", PatchType.PlayerEvent)]
public static class PlayerWorkstationPatch
{
    private static readonly PlayerEvents Player;

    static PlayerWorkstationPatch()
    {
        Player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorkstationController), nameof(WorkstationController.ServerInteract))]
    public static bool OnStay(WorkstationController __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            if (colliderId != __instance._activateCollder.ColliderId || __instance.Status != 0 || ply == null)
                return false;

            var ev = new StartWorkStationEvent(ply, true, __instance.GetSynapseWorkStation());
            if (ev.Player == null || ev.WorkStation == null) return false;
            Player.StartWorkStation.RaiseSafely(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("ServerInteract Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlaceBulletHole", PatchType.PlayerEvent)]
public static class PlaceBulletHolePatch
{
    private static readonly PlayerEvents Player;

    static PlaceBulletHolePatch()
    {
        Player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBulletholeDecal))]
    public static bool PlaceBulletHole(StandardHitregBase __instance, RaycastHit hit)
    {
        try
        {
            var player = __instance.Hub.GetSynapsePlayer();
            if (player == null) return false;

            var ev = new PlaceBulletHoleEvent(player, true, hit.point);
            Player.PlaceBulletHole.RaiseSafely(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Place BulletHole Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerJoin", PatchType.PlayerEvent)]
public static class PlayerJoinPatch
{
    private static readonly PlayerEvents Player;
    static PlayerJoinPatch() => Player = Synapse.Get<PlayerEvents>();


    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static void OnJoin(NicknameSync __instance, ref string nick)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player.PlayerType == PlayerType.Server) return;
            var ev = new JoinEvent(__instance.GetSynapsePlayer(), nick);
            Player.Join.RaiseSafely(ev);
            nick = ev.NickName;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player Join Patch failed\n" + ex);
        }
    }
}

[Automatic]
[SynapsePatch("PlayerDropItem", PatchType.PlayerEvent)]
public static class PlayerDropItemPatch
{
    private static readonly PlayerEvents Player;

    static PlayerDropItemPatch()
    {
        Player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem))]
    public static bool OnDropItem(Inventory __instance, ref ushort itemSerial, ref bool tryThrow)
    {
        try
        {
            if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var itemBase) || !itemBase.CanHolster())
                return false;

            var ev = new DropItemEvent(__instance.GetSynapsePlayer(), true, itemBase.GetItem(), tryThrow);
            Player.DropItem.RaiseSafely(ev);
            tryThrow = ev.Throw;
            itemSerial = ev.ItemToDrop.Serial;
            return ev.Allow && ev.Player.Inventory.Items.Contains(ev.ItemToDrop);
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player Drop Item Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerDeath", PatchType.PlayerEvent)]
public static class PlayerDeathPatch
{
    private static readonly SynapseTranslation Translation;
    private static readonly PlayerService Player;
    private static readonly PlayerEvents PlayerEvent;

    static PlayerDeathPatch()
    {
        Translation = Synapse.Get<SynapseConfigService>().Translation;
        PlayerEvent = Synapse.Get<PlayerEvents>();
        Player = Synapse.Get<PlayerService>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    public static bool OnKill(PlayerStats __instance, DamageHandlerBase handler)
    {
        try
        {
            if (handler == null) return false;
            var damage = (handler as StandardDamageHandler)?.Damage ?? 0;
            var victim = __instance.GetSynapsePlayer();
            var attacker = (handler as AttackerDamageHandler)?.Attacker.GetSynapsePlayer();
            var damageType = handler.GetDamageType();

            if (damageType == DamageType.PocketDecay)
                attacker = Player.Players.FirstOrDefault(x =>
                    x.MainScpController.Scp106.PlayersInPocket.Contains(victim));

            var playerMsg = "";

            if (attacker?.CustomRole != null)
            {
                var translation = Translation.Get(victim).DeathMessage.Replace("\\n", "\n");
                playerMsg = string.Format(translation, attacker.DisplayName, attacker.RoleName);
            }

            var ev = new DeathEvent(victim, true, attacker, damageType, damage, playerMsg, null);
            PlayerEvent.Death.RaiseSafely(ev);

            if (!ev.Allow)
            {
                victim.Health = 1;
                return false;
            }

            playerMsg = ev.DeathMessage;

            RagdollManager.ServerSpawnRagdoll(victim.Hub,
                !string.IsNullOrWhiteSpace(ev.RagDollInfo) ? new CustomReasonDamageHandler(ev.RagDollInfo) : handler);

            victim.Inventory.DropEverything();
            victim.RoleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.Died);

            if (!string.IsNullOrWhiteSpace(ev.DeathMessage))
                handler = new CustomReasonDamageHandler(playerMsg);

            if (victim.CurrentRole is SpectatorRole spectatorRole)
            {
                spectatorRole.ServerSetData(handler);
            }

            victim.GameConsoleTransmission.SendToClient("You died. Reason: " + handler.ServerLogsText, "yellow");

            foreach (var larry in Player.Players)
            {
                var playerPocket = larry.MainScpController.Scp106.PlayersInPocket;
                if (playerPocket.Contains(victim))
                    playerPocket.Remove(victim);
            }

            victim.RemoveCustomRole(DeSpawnReason.Death);
            if (victim.PlayerType != PlayerType.Dummy) return false;

            if (victim is DummyPlayer { DestroyWhenDied: true } dummy)
                dummy.SynapseDummy?.Destroy();
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Kill Player Patch failed\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("SetClass", PatchType.PlayerEvent)]
public static class SetClassPatch
{
    private static readonly PlayerEvents PlayerEvents;
    static SetClassPatch() => PlayerEvents = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
    public static bool SetClass(PlayerRoleManager __instance, ref RoleTypeId newRole,
        ref RoleChangeReason reason, ref RoleSpawnFlags spawnFlags)
    {
        try
        {
            var ev = new SetClassEvent(__instance.Hub.GetSynapsePlayer(), newRole, reason, spawnFlags);
            PlayerEvents.SetClass.RaiseSafely(ev);
            newRole = ev.Role;
            reason = ev.SpawnReason;
            spawnFlags = ev.SpawnFlags;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("SetClass Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("LockerInteract", PatchType.PlayerEvent)]
public static class LockerInteractPatch
{
    private static readonly PlayerEvents Player;
    static LockerInteractPatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Locker), nameof(Locker.ServerInteract))]
    public static bool LockerInteract(Locker __instance, ReferenceHub ply, byte colliderId)
    {
        try
        {
            if (colliderId >= __instance.Chambers.Length || !__instance.Chambers[colliderId].CanInteract) return false;
            var player = ply.GetSynapsePlayer();
            var hasPerms = __instance.Chambers[colliderId].RequiredPermissions.CheckPermission(player, true);
            var locker = __instance.GetSynapseLocker();
            var ev = new LockerUseEvent(player, hasPerms, locker, locker.Chambers[colliderId])
            {
                Allow = EventManager.ExecuteEvent(ServerEventType.PlayerInteractLocker, ply, __instance,
                    __instance.Chambers[colliderId], hasPerms)
            };
            Player.LockerUse.RaiseSafely(ev);

            if (!ev.Allow) return false;

            if (!ev.IsAllowedToOpen)
            {
                __instance.RpcPlayDenied(colliderId);
                return false;
            }

            __instance.Chambers[colliderId].SetDoor(!__instance.Chambers[colliderId].IsOpen, __instance._grantedBeep);
            __instance.RefreshOpenedSyncvar();
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Locker Interact Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("DropAmmo", PatchType.PlayerEvent)]
public static class DropAmmoPatch
{
    private static readonly PlayerEvents Player;
    static DropAmmoPatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerDropAmmo))]
    public static bool DropAmmo(Inventory inv, ref ItemType ammoType, ref ushort amount, ref bool checkMinimals)
    {
        try
        {
            var player = inv._hub.GetSynapsePlayer();
            if (player == null || !Enum.IsDefined(typeof(AmmoType), (AmmoType)ammoType)) return true;
            var ev = new DropAmmoEvent(player, true, (AmmoType)ammoType, amount, checkMinimals);
            Player.DropAmmo.RaiseSafely(ev);
            ammoType = (ItemType)ev.AmmoType;
            amount = ev.Amount;
            checkMinimals = ev.CheckMinimals;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player Drop Ammo Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("ReportPlayer", PatchType.PlayerEvent)]
public static class ReportPlayerPatch
{
    private static readonly PlayerService PlayerService;
    private static readonly PlayerEvents Player;

    static ReportPlayerPatch()
    {
        PlayerService = Synapse.Get<PlayerService>();
        Player = Synapse.Get<PlayerEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.UserCode_CmdReport))]
    public static bool OnReport(CheaterReport __instance, uint playerNetId, ref string reason, ref bool notifyGm)
    {
        try
        {
            if (reason == null) return false;
            if (Time.time - __instance._lastReport < 2f) return true;
            var reported = PlayerService.GetPlayer(playerNetId);
            var player = __instance.GetSynapsePlayer();
            var ev = new ReportEvent(player, true, reported, reason, notifyGm);
            Player.Report.RaiseSafely(ev);
            reason = ev.Reason;
            notifyGm = ev.SendToNorthWood;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Report Event Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerBan&Kick", PatchType.PlayerEvent)]
public static class PlayerBanPatch
{
    private static readonly PlayerEvents Player;
    static PlayerBanPatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser),
        new[] { typeof(ReferenceHub), typeof(ICommandSender), typeof(string), typeof(long) })]
    public static bool Ban(ReferenceHub target, ICommandSender issuer, ref string reason, ref long duration)
    {
        try
        {
            if (duration == 0)
            {
                return true;
            }

            if (target.serverRoles.BypassStaff) return false;
            var player = target.GetSynapsePlayer();
            var admin = (issuer as CommandSender)?.GetSynapsePlayer();
            if (player == null || admin == null) return true;
            var ev = new BanEvent(player, true, admin, reason, duration);
            Player.Ban.RaiseSafely(ev);
            reason = ev.Reason;
            duration = ev.Duration;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player Ban Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.KickUser),
        new[] { typeof(ReferenceHub), typeof(ICommandSender), typeof(string) })]
    public static bool Kick(ReferenceHub target, ICommandSender issuer, ref string reason)
    {
        try
        {
            var player = target.GetSynapsePlayer();
            var admin = (issuer as CommandSender)?.GetSynapsePlayer();
            if (player == null || admin == null) return true;
            var ev = new KickEvent(player, admin, reason, true);
            Player.Kick.RaiseSafely(ev);
            reason = ev.Reason;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player Kick Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("SendPlayerData", PatchType.PlayerEvent)]
public static class SendPlayerDataPatch
{
    private static readonly PlayerEvents Player;
    static SendPlayerDataPatch() => Player = Synapse.Get<PlayerEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.GetNewSyncData))]
    public static bool GetNewData(ReferenceHub receiver, ReferenceHub target, FirstPersonMovementModule fpmm,
        bool isInvisible, out FpcSyncData __result)
    {
        try
        {
            __result = default;
            var prevSyncData = FpcServerPositionDistributor.GetPrevSyncData(receiver, target);
            var player = receiver.GetSynapsePlayer();
            var targetPlayer = target.GetSynapsePlayer();
            if (player == null || targetPlayer == null) return true;
            var ev = new SendPlayerDataEvent(player, targetPlayer)
            {
                Position = targetPlayer.Position,
                IsGrounded = fpmm.IsGrounded,
                IsInvisible = isInvisible,
                MovementState = fpmm.SyncMovementState
            };

            switch (targetPlayer.Invisible)
            {
                case InvisibleMode.Full:
                case InvisibleMode.Alive or InvisibleMode.Ghost when player.IsAlive:
                case InvisibleMode.Admin when !player.HasPermission("synapse.see.invisible"):
                case InvisibleMode.Visual when player.RoleType is not RoleTypeId.Scp079 and not RoleTypeId.Scp939
                    and not RoleTypeId.Scp096 and not RoleTypeId.Spectator:
                    ev.IsInvisible = true;
                    break;
            }

            Player.SendPlayerData.RaiseSafely(ev);

            var syncData = ev.IsInvisible
                ? default
                : new FpcSyncData(prevSyncData, ev.MovementState, ev.IsGrounded,
                    new RelativePosition(ev.Position), fpmm.MouseLook);

            SynapseLogger<SynapsePlayer>.Info(syncData._position.Position);

            FpcServerPositionDistributor.PreviouslySent[receiver.netId][target.netId] = syncData;
            __result = syncData;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Player SendPlayerData Patch failed\n" + ex);
            __result = default;
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static bool WriteAll(ReferenceHub receiver, NetworkWriter writer)
    {
        try
        {
            ushort num = 0;
            bool flag;
            VisibilityController visibilityController;
            if (receiver.roleManager.CurrentRole is ICustomVisibilityRole customVisibilityRole)
            {
                flag = true;
                visibilityController = customVisibilityRole.VisibilityController;
            }
            else
            {
                flag = false;
                visibilityController = null;
            }
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.netId != receiver.netId)
                {
                    if (hub.roleManager.CurrentRole is not IFpcRole fpcRole) continue;

                    bool flag2 = flag && !visibilityController.ValidateVisibility(hub);
                    var newSyncData = GetSyncData(receiver, hub, fpcRole.FpcModule, flag2, out var canSee);
                    if (!flag2 && canSee)
                    {
                        FpcServerPositionDistributor._bufferPlayerIDs[(int)num] = hub.PlayerId;
                        FpcServerPositionDistributor._bufferSyncData[(int)num] = newSyncData;
                        num += 1;
                    }
                }
                else
                {
                    if (hub.roleManager.CurrentRole is not IFpcRole fpcRole) continue;
                    var player = hub.GetSynapsePlayer();
                    if (!player.refreshHorizontalRotation && !player.refreshVerticalRotation) continue;
                    var newSyncData = GetSelfPlayerData(player, fpcRole.FpcModule);
                    FpcServerPositionDistributor._bufferPlayerIDs[(int)num] = hub.PlayerId;
                    FpcServerPositionDistributor._bufferSyncData[(int)num] = newSyncData;
                    num += 1;
                }
            }
            writer.WriteUInt16(num);
            for (int i = 0; i < (int)num; i++)
            {
                writer.WriteRecyclablePlayerId(new RecyclablePlayerId(FpcServerPositionDistributor._bufferPlayerIDs[i]));
                FpcServerPositionDistributor._bufferSyncData[i].Write(writer);
            }
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Send Player Data Patch failed\n" + ex);
        }

        return false;
    }
    
    private static FpcSyncData GetSelfPlayerData(SynapsePlayer player, FirstPersonMovementModule firtstPersonModule)
    {
        if (player.refreshHorizontalRotation)
        {
            firtstPersonModule.MouseLook.CurrentHorizontal = player.horizontalRotation;
            player.refreshHorizontalRotation = false;
        }
        if (player.refreshVerticalRotation)
        { 
            firtstPersonModule.MouseLook.CurrentVertical = player.verticalRotation;
            player.refreshVerticalRotation = false;
        }


        var data = new FpcSyncData(default, 
        firtstPersonModule.SyncMovementState,
        firtstPersonModule.IsGrounded, 
        new RelativePosition(Vector3.zero), 
        firtstPersonModule.MouseLook);
        return data;
    }

    private static FpcSyncData GetSyncData(ReferenceHub receiver, ReferenceHub target,
        FirstPersonMovementModule firtstPersonModule, bool isInvisible, out bool canSee)
    {
        var prevSyncData = FpcServerPositionDistributor.GetPrevSyncData(receiver, target);
        var player = receiver.GetSynapsePlayer();
        var targetPlayer = target.GetSynapsePlayer();
        if (player == null || targetPlayer == null)
        {
            canSee = false;
            return default;
        }
        var ev = new SendPlayerDataEvent(player, targetPlayer)
        {
            Position = targetPlayer.Position,
            IsGrounded = firtstPersonModule.IsGrounded,
            IsInvisible = isInvisible,
            MovementState = firtstPersonModule.SyncMovementState
        };

        switch (targetPlayer.Invisible)
        {
            case InvisibleMode.Full:
            case InvisibleMode.Alive or InvisibleMode.Ghost when player.IsAlive:
            case InvisibleMode.Admin when !player.HasPermission("synapse.see.invisible"):
            case InvisibleMode.Visual when player.RoleType is not RoleTypeId.Scp079 and not RoleTypeId.Scp939
                and not RoleTypeId.Scp096 and not RoleTypeId.Spectator:
                ev.IsInvisible = true;
                break;
        }

        Player.SendPlayerData.RaiseSafely(ev);

        var syncData = ev.IsInvisible
            ? default
            : new FpcSyncData(prevSyncData, ev.MovementState, ev.IsGrounded,
                new RelativePosition(ev.Position), firtstPersonModule.MouseLook);

        FpcServerPositionDistributor.PreviouslySent[receiver.netId][target.netId] = syncData;
        canSee = !ev.IsInvisible;
        return syncData;
    }
}

[Automatic]
[SynapsePatch("Pickup",PatchType.PlayerEvent)]
public static class PickupPatches
{
    private static readonly PlayerEvents Player;
    static PickupPatches() => Player = Synapse.Get<PlayerEvents>();

    private static void ValidateStart(SearchCompletor searchCompletor, ref bool allow)
    {
        try
        {
            var player = searchCompletor.Hub.GetSynapsePlayer();
            var item = searchCompletor.TargetPickup.GetItem();
            var ev = new PickupEvent(player.GetSynapsePlayer(), allow, item);
            Player.Pickup.RaiseSafely(ev);
            allow = ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Search Completor Validate Start Patch failed\n" + ex);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ArmorSearchCompletor), nameof(ArmorSearchCompletor.ValidateStart))]
    public static void ValidateArmo(ArmorSearchCompletor __instance, ref bool __result) =>
        ValidateStart(__instance, ref __result);
    

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.ValidateStart))]
    public static void ValidateItem(ItemSearchCompletor __instance, ref bool __result) =>
        ValidateStart(__instance, ref __result);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SearchCompletor), nameof(SearchCompletor.ValidateStart))]
    public static void ValidateDefault(SearchCompletor __instance, ref bool __result)
    {
        if(__instance.GetType() == typeof(ArmorSearchCompletor) || __instance.GetType() == typeof(ItemSearchCompletor)) return;
        ValidateStart(__instance, ref __result);
    }
}

#endif