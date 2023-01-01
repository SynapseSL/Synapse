using CommandSystem;
using HarmonyLib;
using Hazards;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using MEC;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Ragdolls;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using PlayerStatsSystem;
using RemoteAdmin;
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
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

namespace Synapse3.SynapseModule.Patching.Patches;

[Automatic]
[SynapsePatch("ScpVoice", PatchType.PlayerEvent)]
public static class ScpVoicePatch
{

    [HarmonyPrefix]
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static bool OnServerReceiveMessage(NetworkConnection conn, VoiceMessage msg)
    {
        if (msg.SpeakerNull) return false;

        var player = msg.Speaker.GetSynapsePlayer();

        if (msg.SpeakerNull
            || player.NetId != conn.identity.netId
            || player.CurrentRole is not IVoiceRole voiceRoleSpeaker
            || !voiceRoleSpeaker.VoiceModule.CheckRateLimit())
        {
            return false;
        }

        var flags = VoiceChatMutes.GetFlags(msg.Speaker);
        if (flags == VcMuteFlags.GlobalRegular || flags == VcMuteFlags.LocalRegular)
        {
            return false;
        }

        var voiceChatChannel = voiceRoleSpeaker.VoiceModule.ValidateSend(msg.Channel);
        if (voiceChatChannel == VoiceChatChannel.None)
        {
            return false;
        }

        voiceRoleSpeaker.VoiceModule.CurrentChannel = voiceChatChannel;

        if (player.ScpController.ProximityChat && voiceChatChannel == VoiceChatChannel.ScpChat)
        {
            voiceChatChannel = VoiceChatChannel.Proximity;

            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.roleManager.CurrentRole is IVoiceRole voiceRoleRecever 
                    && Vector3.Distance(player.Position, hub.transform.position) < 7)//Change this to a decreases the voice
                {
                    var voiceChatChannel2 = voiceRoleRecever.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                    if (voiceChatChannel2 != 0)
                    {
                        msg.Channel = voiceChatChannel2;
                        hub.connectionToClient.Send(msg);
                    }
                }
            }
            return false;
        }

        foreach (ReferenceHub hub in ReferenceHub.AllHubs)
        {
            if (hub.roleManager.CurrentRole is IVoiceRole voiceRoleRecever)
            {
                var voiceChatChannel2 = voiceRoleRecever.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                if (voiceChatChannel2 != 0)
                {
                    msg.Channel = voiceChatChannel2;
                    hub.connectionToClient.Send(msg);
                }
            }
        }
        return false;
    }
}

[Automatic]
[SynapsePatch("PlayerDoorInteract", PatchType.PlayerEvent)]
public static class SendPlayerDataPatch
{

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
            if (__instance.ActiveLocks > 0)
            {
                var mode = DoorLockUtils.GetMode((DoorLockReason)__instance.ActiveLocks);

                var canInteractGeneral = mode.HasFlagFast(DoorLockMode.CanClose) || mode.HasFlagFast(DoorLockMode.CanOpen);
                var scpOverride = mode.HasFlagFast(DoorLockMode.ScpOverride) && player.Team == Team.SCPs;
                var canChangeCurrently = mode != DoorLockMode.FullLock 
                    && (__instance.TargetState && mode.HasFlagFast(DoorLockMode.CanClose) 
                    || !__instance.TargetState && mode.HasFlagFast(DoorLockMode.CanOpen));

                if (!canInteractGeneral && !scpOverride && !canChangeCurrently)
                {
                    bypassDenied = true;
                }
            }

            //This is most often false when the Animation is still playing
            if (!door.AllowInteracting(player, colliderId)) return false;

            if (!bypassDenied)
            {
                if (player.RoleType == RoleTypeId.Scp079 
                    || door.RequiredPermissions.CheckPermission(player))
                {
                    allow = true;
                }
            }

            var ev = new DoorInteractEvent(player, allow, door.GetSynapseDoor(), bypassDenied);
            Synapse.Get<PlayerEvents>().DoorInteract.Raise(ev);

            if (ev.Allow)
            {
                door.NetworkTargetState = !door.TargetState;
                door._triggerPlayer = player;
                return false;
            }

            if (ev.LockBypassRejected)
            {
                door.LockBypassDenied(player, colliderId);
                return false;
            }

            if (ev.PlayDeniedSound)
                door.PermissionsDenied(player, colliderId);
            DoorEvents.TriggerAction(door, DoorAction.AccessDenied, player);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: PlayerDoorInteract failed!!\n{e}");
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
        var player = hub.GetSynapsePlayer();
        if (player == null) return false;
        player.TriggerEscape(false);
        return false;
    }
}

[Automatic]
[SynapsePatch("PlayerNameChange", PatchType.PlayerEvent)]
public static class PlayerNameChangePatch
{
    static PlayerEvents _player;

    static PlayerNameChangePatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.DisplayName), MethodType.Setter)]
    public static void SetDisplayName(NicknameSync __instance)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null) return;
            var ev = new UpdateDisplayNameEvent(player, __instance.DisplayName);
            Synapse.Get<PlayerEvents>().UpdateDisplayName.Raise(ev);
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: PlayerNameChange failed!!\n{e}");
        }
    }
}

[Automatic]
[SynapsePatch("PlayerHeal", PatchType.PlayerEvent)]
public static class PlayerHealPatch
{
    static PlayerEvents _player;

    static PlayerHealPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthStat), nameof(HealthStat.ServerHeal))]
    public static bool OnServerHeal(HealthStat __instance, float healAmount)
    {
        try
        {
            var player = __instance.Hub.GetSynapsePlayer();
            var ev = new HealEvent(player, true, healAmount);
            _player.Heal.RaiseSafely(ev);
            return ev.Allow;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: PlayerHeal failed!!\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerOpenWarHeadButton", PatchType.PlayerEvent)]
public static class PlayerOpenWarHeadButtonPatch
{
    private static readonly SynapseConfigService _config;
    static PlayerEvents _player;

    static PlayerOpenWarHeadButtonPatch()
    {
        _config = Synapse.Get<SynapseConfigService>();
        _player = Synapse.Get<PlayerEvents>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdSwitchAWButton))]
    public static bool OnWarHeadButton(PlayerInteract __instance)
    {
        try
        {
            if (!__instance.CanInteract) return false;
            var gameObject = GameObject.Find("OutsitePanelScript");
            var player = __instance.GetSynapsePlayer();

            var componentInParent = gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>();
            if (componentInParent == null || (componentInParent.keycardEntered && !_config.GamePlayConfiguration.WarheadButtonClosable))
                return false;

            var allow = KeycardPermissions.AlphaWarhead.CheckPermission(player);

            var ev = new OpenWarheadButtonEvent(player, allow, componentInParent.NetworkkeycardEntered);
            Synapse.Get<PlayerEvents>().OpenWarheadButton.Raise(ev);

            if (!ev.Allow) return false;

            __instance.OnInteract();
            componentInParent.NetworkkeycardEntered = !componentInParent.NetworkkeycardEntered;
            SpawnableTeamType stt;
            if (!__instance._hub.TryGetAssignedSpawnableTeam(out stt))
                return false;
            RespawnTokensManager.GrantTokens(stt, 1f);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: PlayerOpenWarHeadButton failed!!\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerWarHeadInteract", PatchType.PlayerEvent)]
public static class WarHeadInteractPatch
{
    static PlayerEvents _player;
    static NukeService _nuck;

    static WarHeadInteractPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
        _nuck = Synapse.Get<NukeService>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.UserCode_CmdUsePanel))]
    public static bool OnPanelInteract(PlayerInteract __instance, ref PlayerInteract.AlphaPanelOperations n)
    {
        try
        {
            if (!__instance.CanInteract) return false;

            var player = __instance.GetSynapsePlayer();
            var nukeside = AlphaWarheadOutsitePanel.nukeside;

            if (!__instance.ChckDis(player.Position))
                return false;

            var ev = new WarheadPanelInteractEvent(player, !_nuck.InsidePanel.Locked, n);

            _player.WarheadPanelInteract.Raise(ev);
            n = ev.Operation;

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlaceWarHeadInteract failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerTrantumHazard", PatchType.PlayerEvent)]
public static class PlayerTrantumHazardPatch
{
    static PlayerEvents _player;

    static PlayerTrantumHazardPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }


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
            Synapse.Get<PlayerEvents>().WalkOnTantrum.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerTrantumHazard failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerSinkHoleHazard", PatchType.PlayerEvent)]
public static class PlayerSinkHoleHazardPatch
{
    static PlayerEvents _player;

    static PlayerSinkHoleHazardPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }

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
            _player.WalkOnSinkhole.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerTrantumHazard failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerNickname", PatchType.PlayerEvent)]
public static class PlayerNicknamePatch
{
    static PlayerEvents _player;

    static PlayerNicknamePatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static void OnSetNick(NicknameSync __instance, ref string nick)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player == null || player.PlayerType == PlayerType.Server) return;
            var ev = new JoinEvent(__instance.GetSynapsePlayer(), nick);
            _player.Join.RaiseSafely(ev);
            nick = ev.NickName;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerNickname failed\n" + ex);
            return;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerWorkstation", PatchType.PlayerEvent)]
public static class PlayerWorkstationPatch
{
    static PlayerEvents _player;

    static PlayerWorkstationPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
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
            _player.StartWorkStation.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: ServerInteract failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlaceBulletHole", PatchType.PlayerEvent)]
public static class PlaceBulletHolePatch
{
    static PlayerEvents _player;

    static PlaceBulletHolePatch()
    {
        _player = Synapse.Get<PlayerEvents>();
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
            _player.PlaceBulletHole.Raise(ev);

            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlaceBulletHole failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerKick", PatchType.PlayerEvent)]
public static class PlayerKickPatch
{
    static PlayerEvents _player;

    static PlayerKickPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.KickUser), 
        typeof(ReferenceHub), typeof(ICommandSender), typeof(string))]
    public static bool OnJoin(ReferenceHub target, ICommandSender issuer, string reason)
    {
        try
        {
            var playerIssuer = (issuer as PlayerCommandSender)?.GetSynapsePlayer();

            var ev = new KickEvent(target?.GetSynapsePlayer(), playerIssuer, reason, true);

            _player.Kick.RaiseSafely(ev);

            if (!ev.Allow) return false;
            return true;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerKick failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerJoin", PatchType.PlayerEvent)]
public static class PlayerJoinPatch
{
    static PlayerEvents _player;

    static PlayerJoinPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static void OnJoin(NicknameSync __instance, ref string nick)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (player.PlayerType == PlayerType.Server) return;
            var ev = new JoinEvent(__instance.GetSynapsePlayer(), nick);
            _player.Join.Raise(ev);
            nick = ev.NickName;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerJoin failed\n" + ex);
        }
    }
}

[Automatic]
[SynapsePatch("PlayerDropItem", PatchType.PlayerEvent)]
public static class PlayerDropItemPatch
{
    static PlayerEvents _player;

    static PlayerDropItemPatch()
    {
        _player = Synapse.Get<PlayerEvents>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem))]
    public static bool OnDropItem(Inventory __instance, ref ushort itemSerial, ref bool tryThrow)
    {
        try
        {
            if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var itembase) || !itembase.CanHolster())
                return false;

            var ev = new DropItemEvent(__instance.GetSynapsePlayer(), true, itembase.GetItem(), tryThrow);
            _player.DropItem.Raise(ev);
            tryThrow = ev.Throw;
            itemSerial = ev.ItemToDrop.Serial;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: PlayerDropItem failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerDeath", PatchType.PlayerEvent)]
public static class PlayerDeathPatch
{
    static SynapseTranslation _translation;
    static PlayerService _player;
    static PlayerEvents _playerEvent;

    static PlayerDeathPatch()
    {
        _translation = Synapse.Get<SynapseConfigService>().Translation;
        _playerEvent = Synapse.Get<PlayerEvents>();
        _player = Synapse.Get<PlayerService>();
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.KillPlayer))]
    public static bool OnKill(PlayerStats __instance, DamageHandlerBase handler)
    {
        try
        {
            var damage = (handler as StandardDamageHandler)?.Damage ?? 0;
            var victim = __instance.GetSynapsePlayer();
            var attacker = (handler as AttackerDamageHandler)?.Attacker.GetSynapsePlayer();
            var damageType = handler.GetDamageType();

            if (damageType == DamageType.PocketDecay)
                attacker = _player.Players.FirstOrDefault(x => x.ScpController.Scp106.PlayersInPocket.Contains(victim));

            string playerMsg = null;

            if (attacker?.CustomRole != null)
            {
                var translation = victim.GetTranslation(_translation).DeathMessage.Replace("\\n", "\n");
                playerMsg = string.Format(translation, attacker.DisplayName, attacker.RoleName);
            }

            var ev = new DeathEvent(victim, true, attacker, damageType, damage, playerMsg, null);
            _playerEvent.Death.Raise(ev);

            if (!ev.Allow)
            {
                victim.Health = 1;
                return false;
            }

            victim.DeathPosition = victim.Position;

            playerMsg = ev.DeathMessage;
            var ragdollInfo = ev.RagdollInfo;

            //--Vanila Stuff rework--
            if (ragdollInfo != null)
                RagdollManager.ServerSpawnRagdoll(victim, new CustomReasonDamageHandler(ragdollInfo));
            else
                RagdollManager.ServerSpawnRagdoll(victim, handler);

            victim.Inventory.DropEverything();

            var classManager = victim.ClassManager;
            victim.RoleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.Died);
            if (playerMsg != null)
                handler = new CustomReasonDamageHandler(playerMsg);
            if (__instance._hub.roleManager.CurrentRole is SpectatorRole spectatorRole)
            {
                spectatorRole.ServerSetData(handler);
            }
            victim.GameConsoleTransmission.SendToClient("You died. Reason: " + handler.ServerLogsText, "yellow");

            //--Synapse API--
            foreach (var larry in _player.Players)
            {
                var playerPocket = larry.ScpController.Scp106.PlayersInPocket;
                if (playerPocket.Contains(victim))
                    playerPocket.Remove(victim);
            }

            if (victim.PlayerType == PlayerType.Dummy)
            {
                Timing.CallDelayed(Timing.WaitForOneFrame, () =>
                {
                    var dummy = victim as DummyPlayer;
                    if (dummy != null && dummy.DestroyWhenDied)
                        dummy.SynapseDummy.Destroy();
                });
            }

            victim.RemoveCustomRole(DeSpawnReason.Death);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Sy3 Event: PlayerDeathPatch failed!!\n{e}");

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
    public static bool SetClass(PlayerRoleManager __instance, out SetClassEvent __state, ref RoleTypeId newRole,
        ref RoleChangeReason reason)

    {
        __state = new SetClassEvent(__instance.Hub.GetSynapsePlayer(), newRole, reason);
        if (PlayerRoleLoader.TryGetRoleTemplate<FpcStandardRoleBase>(newRole, out var rolePrefab) &&
            rolePrefab.SpawnpointHandler != null &&
            rolePrefab.SpawnpointHandler.TryGetSpawnpoint(out var pos, out var rot))
        {
            __state.Position = pos;
            __state.HorizontalRotation = rot;
        }

        PlayerEvents.SetClass.RaiseSafely(__state);
        newRole = __state.Role;
        reason = __state.SpawnReason;
        return __state.Allow;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.ServerSetRole))]
    public static void PostSetClass(SetClassEvent __state)
    {
        if (!typeof(FpcStandardRoleBase).IsAssignableFrom(FakeRoleManager.EnumToType[__state.Role])) return;
        
        __state.Player.Hub.transform.position = __state.Position;
        __state.Player.FirstPersonMovement.MouseLook.CurrentHorizontal = __state.HorizontalRotation;
    }
}
