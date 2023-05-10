using Achievements;
using CustomPlayerEffects;
using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.MicroHID;
using MapGeneration;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using Subtitles;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using PlayerRoles.PlayableScps.Scp079.Pinging;
using UnityEngine;
using Utils.Networking;
using Utils.NonAllocLINQ;
using VoiceChat;
using static PlayerRoles.PlayableScps.Scp173.Scp173TeleportAbility;
using static PocketDimensionTeleport;

namespace Synapse3.SynapseModule.Patching.Patches;

#if !PATCHLESS

[Automatic]
[SynapsePatch("Scp079Recontain", PatchType.ScpEvent)]
public static class Scp079RecontainPatch
{
    private static readonly ScpEvents Scp;
    static Scp079RecontainPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.UpdateStatus))]
    public static bool UpdateContain079Status(Scp079Recontainer __instance, int engagedGenerators)
    {
        try
        {
            var count = Scp079Recontainer.AllGenerators.Count;
            var announcement = string.Format(__instance._announcementProgress, engagedGenerators, count);
            var parts = new List<SubtitlePart>
            {
                new(SubtitleType.GeneratorsActivated, new[] { engagedGenerators.ToString(), count.ToString() })
            };

            if (engagedGenerators >= count)
            {
                var ev = new Scp079ContainEvent(Scp079ContainmentStatus.OverrideDoors);
                Scp.Scp079Contain.RaiseSafely(ev);
                if (!ev.Allow) return false;

                announcement += __instance._announcementAllActivated;
                __instance.SetContainmentDoors(true, Scp079Role.ActiveInstances.Count > 0);
                parts.Add(new SubtitlePart(SubtitleType.AllGeneratorsEngaged, null));
            }

            new SubtitleMessage(parts.ToArray()).SendToAuthenticated();
            __instance.PlayAnnouncement(announcement, 1f);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079Contain (OverrideDoors) Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.EndOvercharge))]
    public static void End079Contain()
    {
        try
        {
            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.Finished);
            Scp.Scp079Contain.Raise(ev);
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079Contain (End) Event Failed\n" + ex);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.Recontain))]
    public static bool Start079Contain()
    {
        try
        {
            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.AnnounceOvercharge);
            Scp.Scp079Contain.RaiseSafely(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Sy3 Event: Scp079 Contain (Start) Event Failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.RefreshActivator))]
    public static bool Refresh079ContainActivator(Scp079Recontainer __instance)
    {
        try
        {
            if (!__instance._delayStopwatch.IsRunning ||
                __instance._delayStopwatch.Elapsed.TotalSeconds <= __instance._activationDelay) return true;

            var ev = new Scp079ContainEvent(Scp079ContainmentStatus.Overcharge);
            Scp.Scp079Contain.RaiseSafely(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079Contain (Refresh) Event Failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079ChangeCamera", PatchType.ScpEvent)]
public static class Scp079ChangeCameraPatch
{
    private static readonly ScpEvents Scp;
    static Scp079ChangeCameraPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079CurrentCameraSync), nameof(Scp079CurrentCameraSync.ServerProcessCmd))]
    public static bool SwitchCamera(Scp079CurrentCameraSync __instance, NetworkReader reader)
    {
        try
        {
            __instance._clientSwitchRequest = (Scp079CurrentCameraSync.ClientSwitchState)reader.ReadByte();
            __instance._requestedCamId = reader.ReadUShort();
            if (__instance._clientSwitchRequest != 0)
            {
                __instance.ServerSendRpc(x => x.roleManager.CurrentRole is SpectatorRole);
                return false;
            }

            if (!Scp079InteractableBase.TryGetInteractable(__instance._requestedCamId, out __instance._switchTarget))
            {
                __instance._errorCode = Scp079HudTranslation.InvalidCamera;
                __instance.ServerSendRpc(true);
                return false;
            }

            if (__instance._switchTarget == __instance.CurrentCamera)
            {
                __instance.ServerSendRpc(toAll: true);
                return false;
            }

            var player = __instance.Owner.GetSynapsePlayer();
            var camera = __instance._switchTarget.GetCamera();
            var cost = __instance.GetSwitchCost(__instance._switchTarget);
            var ev = new Scp079SwitchCameraEvent(player, camera, cost, EventManager.ExecuteEvent(
                ServerEventType.Scp079CameraChanged, player.Hub,
                __instance._switchTarget));

            Scp.Scp079SwitchCamera.RaiseSafely(ev);
            cost = ev.Cost;

            if (!ev.Allow)
            {
                __instance._errorCode = Scp079HudTranslation.SignalLost;
                __instance.ServerSendRpc(toAll: true);
                return false;
            }

            if (cost > __instance._auxManager.CurrentAux)
            {
                __instance._errorCode = Scp079HudTranslation.NotEnoughAux;
                __instance.ServerSendRpc(toAll: true);
                return false;
            }

            if (__instance._lostSignalHandler.Lost)
            {
                __instance._errorCode = Scp079HudTranslation.SignalLost;
                __instance.ServerSendRpc(toAll: true);
                return false;
            }

            __instance._auxManager.CurrentAux -= cost;
            __instance._errorCode = Scp079HudTranslation.Zoom;
            __instance.CurrentCamera = __instance._switchTarget;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079 SwitchCamera Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079DoorLock", PatchType.ScpEvent)]
public static class Scp079DoorLockPatch
{
    private static readonly ScpEvents Scp;
    static Scp079DoorLockPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079DoorLockChanger), nameof(Scp079DoorLockChanger.ServerProcessCmd))]
    public static bool DoorLockChange(Scp079DoorLockChanger __instance, NetworkReader reader)
    {
        try
        {
            if (!NetworkServer.spawned.TryGetValue(reader.ReadUInt(), out var netIden)
                || !netIden.TryGetComponent(out __instance.LastDoor) || !__instance.IsReady)
            {
                return false;
            }

            var player = __instance.Owner.GetSynapsePlayer();
            var door = __instance.LastDoor.GetSynapseDoor();

            if (__instance.TargetAction == DoorAction.Locked)
            {
                if (__instance.GetRemainingCooldown(__instance.LastDoor) > 0f ||
                    Scp079LockdownRoomAbility.IsLockedDown(__instance.LastDoor) ||
                    __instance.LostSignalHandler.Lost)
                    return false;
                var cost = __instance.GetCostForDoor(DoorAction.Locked, __instance.LastDoor);
                var ev = new Scp079LockDoorEvent(player, door, false, cost)
                {
                    Allow = EventManager.ExecuteEvent(ServerEventType.Scp079LockDoor, __instance.Owner,
                        __instance.LastDoor)
                };
                Scp.Scp079LockDoor.RaiseSafely(ev);

                cost = ev.Cost;

                if (!ev.Allow)
                    return false;

                if (cost > __instance.AuxManager.CurrentAux)
                    return false;
                __instance.SetDoorLock(__instance.LastDoor, true);
                __instance.RewardManager.MarkRooms(__instance.LastDoor.Rooms);
                InvokeOnServerDoorLocked(__instance.ScpRole, __instance.LastDoor);
                __instance.AuxManager.CurrentAux -= cost;
            }
            else
            {
                var cost = 0;
                var ev = new Scp079LockDoorEvent(player, door, true, cost)
                {
                    Allow = EventManager.ExecuteEvent(ServerEventType.Scp079UnlockDoor, __instance.Owner,
                        __instance.LastDoor)
                };
                Scp.Scp079LockDoor.RaiseSafely(ev);

                cost = ev.Cost;

                if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                    return false;

                __instance.AuxManager.CurrentAux -= cost;
                __instance.SetDoorLock(__instance.LastDoor, false);
            }

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079 SwitchCamera Event Failed\n" + ex);
            return false;
        }
    }

    private static void InvokeOnServerDoorLocked(Scp079Role scp079, DoorVariant door)
        => Synapse3Extensions.RaiseEvent(typeof(Scp079DoorLockChanger),
            nameof(Scp079DoorLockChanger.OnServerDoorLocked), scp079, door);
}

[Automatic]
[SynapsePatch("Scp079DoorInteract", PatchType.ScpEvent)]
public static class Scp079DoorInteractPatch
{
    private static readonly ScpEvents Scp;
    static Scp079DoorInteractPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079DoorStateChanger), nameof(Scp079DoorStateChanger.ServerProcessCmd))]
    public static bool DoorStateChange(Scp079DoorStateChanger __instance, NetworkReader reader)
    {
        try
        {
            if (!NetworkServer.spawned.TryGetValue(reader.ReadUInt(), out var netIden) ||
                !netIden.TryGetComponent(out __instance.LastDoor) ||
                !__instance.IsReady || __instance.LostSignalHandler.Lost) return false;
            var player = __instance.GetSynapsePlayer();
            if (player == null) return false;


            var targetState = __instance.LastDoor.TargetState;
            var door = __instance.LastDoor.GetSynapseDoor();
            var cost = __instance.GetCostForDoor(__instance.TargetAction, __instance.LastDoor);
            var ev = new Scp079DoorInteractEvent(player, door, cost);
            Scp.Scp079DoorInteract.RaiseSafely(ev);
            cost = ev.Cost;
            if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                return false;
            __instance.LastDoor.ServerInteract(player.Hub, 0);
            if (targetState == __instance.LastDoor.TargetState) return false;
            __instance.RewardManager.MarkRooms(__instance.LastDoor.Rooms);
            Synapse3Extensions.RaiseEvent(typeof(Scp079DoorStateChanger),
                nameof(Scp079DoorStateChanger.OnServerDoorToggled), __instance.ScpRole, __instance.LastDoor);
            __instance.AuxManager.CurrentAux -= cost;
            __instance.ServerSendRpc(toAll: false);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079 Scp079DoorLock Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079Speak", PatchType.ScpEvent)]
public static class Scp079SpeakPatch
{
    private static readonly ScpEvents Scp;
    static Scp079SpeakPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079VoiceModule), nameof(Scp079VoiceModule.ValidateSend))]
    public static bool ValidateVoiceChannel(Scp079VoiceModule __instance, ref VoiceChatChannel __result,
        VoiceChatChannel channel)
    {
        try
        {
            if (channel != VoiceChatChannel.Proximity || !__instance._speakerAbility.CanTransmit)
            {
                if (channel != __instance.PrimaryChannel)
                {
                    __result = VoiceChatChannel.None;
                    return false;
                }

                __result = channel;
                return false;
            }

            var player = __instance.Owner.GetSynapsePlayer();
            var speaker = __instance.ProximityPlayback.transform.position;
            var ev = new Scp079SpeakerUseEvent(player, speaker);
            Scp.Scp079SpeakerUse.RaiseSafely(ev);

            __result = ev.Allow ? VoiceChatChannel.Proximity : VoiceChatChannel.None;
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079 Scp079Speaker Event Failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079Tesla", PatchType.ScpEvent)]
public static class Scp079TeslaPatch
{
    private static readonly ScpEvents Scp;
    static Scp079TeslaPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079TeslaAbility), nameof(Scp079TeslaAbility.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp079TeslaAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.IsReady) return false;
            var cam = __instance.CurrentCamSync.CurrentCamera;
            if (!TeslaGateController.Singleton.TeslaGates.TryGetFirst(
                    x => RoomIdUtils.IsTheSameRoom(cam.Position, x.transform.position), out var tesla)) return false;
            var player = __instance.Owner.GetSynapsePlayer();
            var sTesla = tesla.GetSynapseTesla();
            var cost = __instance._cost;
            var ev = new Scp079TeslaInteractEvent(player, sTesla, cost)
            {
                Allow = EventManager.ExecuteEvent(ServerEventType.Scp079UseTesla, __instance.Owner, tesla)
            };
            cost = ev.Cost;

            Scp.Scp079TeslaInteract.RaiseSafely(ev);

            if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                return false;

            __instance.RewardManager.MarkRoom(cam.Room);
            __instance.AuxManager.CurrentAux -= cost;
            tesla.RpcInstantBurst();
            __instance._nextUseTime = NetworkTime.time + __instance._cooldown;
            __instance.ServerSendRpc(false);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079 Scp079Tesla Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079BlackOutRoom", PatchType.ScpEvent)]
public static class Scp079BlackOutRoomPatch
{
    private static readonly ScpEvents Scp;
    private static readonly RoomService Room;

    static Scp079BlackOutRoomPatch()
    {
        Scp = Synapse.Get<ScpEvents>();
        Room = Synapse.Get<RoomService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079BlackoutRoomAbility), nameof(Scp079BlackoutRoomAbility.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp079BlackoutRoomAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.IsReady || __instance.LostSignalHandler.Lost)
            {
                __instance._successfulController = null;
                __instance.ServerSendRpc(toAll: false);
                return false;
            }

            var player = __instance.Owner.GetSynapsePlayer();
            var room = __instance._roomController.Room.GetVanillaRoom();
            var cost = __instance._cost;
            var ev = new Scp079BlackOutRoomEvent(player, room, cost)
            {
                Allow = EventManager.ExecuteEvent(ServerEventType.Scp079BlackoutRoom, __instance.Owner,
                    __instance._roomController.Room)
            };

            Scp.Scp079BlackOutRoom.RaiseSafely(ev);
            cost = ev.Cost;

            if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                return false;

            __instance.AuxManager.CurrentAux -= cost;
            __instance.RewardManager.MarkRoom(__instance._roomController.Room);
            __instance._blackoutCooldowns[__instance._roomController.netId] =
                NetworkTime.time + (double)__instance._cooldown;
            __instance._roomController.ServerFlickerLights(__instance._blackoutDuration);
            __instance._successfulController = __instance._roomController;
            __instance.ServerSendRpc(true);

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079BlackOutRoom Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079BlackOutZone", PatchType.ScpEvent)]
public static class Scp079BlackOutZonePatch
{
    private static readonly ScpEvents Scp;
    static Scp079BlackOutZonePatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079BlackoutZoneAbility), nameof(Scp079BlackoutZoneAbility.ServerProcessCmd))]
    static bool ServerProcessCmd(Scp079BlackoutZoneAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._syncZone = (FacilityZone)reader.ReadByte();
            if (__instance.ErrorCode != 0)
                return false;
            var player = __instance.Owner.GetSynapsePlayer();
            var zone = __instance._syncZone;
            var cost = __instance._cost;
            var ev = new Scp079BlackOutZoneEvent(player, (ZoneType)zone, cost)
            {
                Allow = EventManager.ExecuteEvent(ServerEventType.Scp079BlackoutZone, __instance.Owner,
                    __instance._syncZone)
            };

            Scp.Scp079BlackOutZone.RaiseSafely(ev);

            cost = ev.Cost;

            if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                return false;

            foreach (var light in FlickerableLightController.Instances)
            {
                if (light.Room.Zone == __instance._syncZone)
                {
                    light.ServerFlickerLights(__instance._duration);
                }
            }

            __instance._cooldownTimer.Trigger(__instance._cooldown);
            __instance.AuxManager.CurrentAux -= cost;
            __instance.ServerSendRpc(true);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079BlackOutZone Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079ReleaseAllLocks", PatchType.ScpEvent)]
public static class Scp079ReleaseAllLocksPatch
{
    private static readonly ScpEvents Scp;
    static Scp079ReleaseAllLocksPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079DoorLockReleaser), nameof(Scp079DoorLockReleaser.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp079DoorLockReleaser __instance, NetworkReader reader)
    {
        try
        {
            var cost = 0;
            var player = __instance.Owner.GetSynapsePlayer();
            var ev = new Scp079ReleaseAllLocksEvent(player, cost);

            Scp.Scp079ReleaseAllLocks.RaiseSafely(ev);
            cost = ev.Cost;

            if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                return false;

            __instance.AuxManager.CurrentAux -= cost;
            __instance._lockChanger.ServerUnlockAll();
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp079ReleaseAllLocks Event Failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079LockdownRoom", PatchType.ScpEvent)]
public static class Scp079LockdownRoomPatch
{
    private static readonly ScpEvents Scp;
    static Scp079LockdownRoomPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079LockdownRoomAbility), nameof(Scp079LockdownRoomAbility.ServerProcessCmd))]
    static bool ServerProcessCmd(Scp079LockdownRoomAbility __instance, NetworkReader reader)
    {
        try
        {
            if (__instance.ErrorCode == Scp079HudTranslation.Zoom && !__instance.LostSignalHandler.Lost)
            {
                var cost = __instance._cost;
                var player = __instance.Owner.GetSynapsePlayer();
                var room = __instance.CurrentCamSync.CurrentCamera.Room.GetVanillaRoom();
                var ev = new Scp079LockdownRoomEvent(player, cost, room)
                {
                    Allow = EventManager.ExecuteEvent(ServerEventType.Scp079LockdownRoom, __instance.Owner,
                        __instance.CurrentCamSync.CurrentCamera.Room)
                };
                Scp.Scp079LockdownRoom.RaiseSafely(ev);

                cost = ev.Cost;

                if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                    return false;

                __instance.AuxManager.CurrentAux -= cost;
                __instance.RemainingCooldown = __instance._lockdownDuration + __instance._cooldown;
                __instance.ServerInitLockdown();
            }

            __instance.ServerSendRpc(false);
            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079LockdownRoom Event Failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079ElevatorInteract", PatchType.ScpEvent)]
public static class Scp079ElevatorInteractPatch
{
    private static readonly ScpEvents Scp;
    private static readonly ElevatorService Elevator;

    static Scp079ElevatorInteractPatch()
    {
        Scp = Synapse.Get<ScpEvents>();
        Elevator = Synapse.Get<ElevatorService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079ElevatorStateChanger), nameof(Scp079ElevatorStateChanger.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp079ElevatorStateChanger __instance, NetworkReader reader)
    {
        try
        {
            if (__instance.LostSignalHandler.Lost)
                return false;

            var elevatorGroup = (ElevatorManager.ElevatorGroup)reader.ReadByte();
            if (!ElevatorDoor.AllElevatorDoors.TryGetValue(elevatorGroup, out var doors) ||
                doors.Any(x => x.ActiveLocks != 0))
                return false;

            var curRoom = __instance.CurrentCamSync.CurrentCamera.Room;
            if (!doors.TryGetFirst(x => x.Rooms.Contains(curRoom),
                    out var door))
                return false;

            var targetState = door.TargetState;
            var chamber = door.TargetPanel.AssignedChamber;
            int num = chamber.CurrentLevel + 1;

            var player = __instance.Owner.GetSynapsePlayer();
            var cost = __instance._cost;
            var destination = num % doors.Count;
            var elevator = Elevator.Elevators
                .FirstOrDefault(p => (p.Chamber as SynapseElevatorChamber)?.Chamber == chamber);
            var ev = new Scp079ElevatorInteractEvent(player, cost, elevator, destination);

            Scp.Scp079ElevatorInteract.RaiseSafely(ev);

            cost = ev.Cost;
            destination = ev.Destination % doors.Count;
            if (!ev.Allow || cost > __instance.AuxManager.CurrentAux)
                return false;

            if (!ElevatorManager.TrySetDestination(elevatorGroup, destination)) return false;

            __instance.AuxManager.CurrentAux -= cost;
            doors.ForEach(delegate(ElevatorDoor x) { __instance.RewardManager.MarkRooms(x.Rooms); });
            __instance.ServerSendRpc(false);
            if (targetState)
            {
                Synapse3Extensions.RaiseEvent(typeof(Scp079ElevatorStateChanger),
                    nameof(Scp079ElevatorStateChanger.OnServerElevatorDoorClosed), __instance.ScpRole, door);
            }

            return false;
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>().Error("Scp079ElevatorInteract Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp079Ping", PatchType.ScpEvent)]
public static class Scp079PingPatch
{
    private static readonly ScpEvents Scp;
    static Scp079PingPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp079PingAbility), nameof(Scp079PingAbility.ServerProcessCmd))]
    public static bool OnPing(Scp079PingAbility __instance, NetworkReader reader)
    {
        try
        {
            var player = __instance.GetSynapsePlayer();
            if (!__instance.IsReady || player == null || __instance.LostSignalHandler.Lost) return false;
            var ev = new Scp079PingEvent(__instance.GetSynapsePlayer(), true, __instance._cost,
                (Scp079PingType)reader.ReadByte(), reader.ReadRelativePosition().Position, reader.ReadVector3());
            if ((byte)ev.PingType >= Scp079PingAbility.PingProcessors.Length) return false;
            
            Scp.Scp079Ping.RaiseSafely(ev);
            
            if (!ev.Allow || ev.Cost > __instance.AuxManager.CurrentAux)
                return false;

            __instance._syncProcessorIndex = (byte)ev.PingType;
            __instance._syncPos = new RelativePosition(ev.Position);
            __instance._syncNormal = ev.Normal;
            __instance.ServerSendRpc(x =>
                __instance.ServerCheckReceiver(x, __instance._syncPos.Position, __instance._syncProcessorIndex));
            __instance.AuxManager.CurrentAux -= ev.Cost;
            __instance._rateLimiter.RegisterInput();
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp079Ping Event Failed\n" + ex);
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp096Attack", PatchType.ScpEvent)]
public static class Scp096AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp096AttackPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096HitHandler), nameof(Scp096HitHandler.ProcessHits))]
    public static bool ProcessHits(Scp096HitHandler __instance, ref Scp096HitResult __result, int count)
    {
        try
        {
            var scp096HitResult = Scp096HitResult.None;
            var scp = __instance._scpRole._lastOwner.GetSynapsePlayer();
            for (var i = 0; i < count; i++)
            {
                var collider = Scp096HitHandler.Hits[i];
                __instance.CheckDoorHit(collider);
                if (!collider.TryGetComponent<IDestructible>(out var destructible))
                    continue;

                var layerMask = (int)Scp096HitHandler.SolidObjectMask & ~(1 << collider.gameObject.layer);
                if (Physics.Linecast(__instance._scpRole.CameraPosition, destructible.CenterOfMass, layerMask) ||
                    !__instance._hitNetIDs.Add(destructible.NetworkId))
                    continue;

                if (destructible is BreakableWindow breakableWindow)
                {
                    if (!__instance.DealDamage(breakableWindow, __instance._windowDamage)) continue;

                    scp096HitResult |= Scp096HitResult.Window;
                    Synapse3Extensions.RaiseEvent(__instance, nameof(Scp096HitHandler.OnWindowHit), breakableWindow);
                    continue;
                }

                if (destructible is not HitboxIdentity hitBoxIdentity || !__instance.IsHumanHitbox(hitBoxIdentity))
                    continue;

                var target = hitBoxIdentity.TargetHub?.GetSynapsePlayer();
                var isTarget = __instance._targetCounter.HasTarget(target);
                var damage = isTarget ? __instance._humanTargetDamage : __instance._humanNontargetDamage;

                var ev = new Scp096AttackEvent(scp, target, __instance._damageType, damage);
                _scp.Scp096Attack.RaiseSafely(ev);
                if (!ev.Allow) continue;
                if (!__instance.DealDamage(hitBoxIdentity, ev.Damage)) continue;

                scp096HitResult |= Scp096HitResult.Human;
                Synapse3Extensions.RaiseEvent(__instance, nameof(Scp096HitHandler.OnPlayerHit), target.Hub);
                if (!target.Hub.IsAlive())
                {
                    scp096HitResult |= Scp096HitResult.Lethal;
                }
            }

            __instance.HitResult |= scp096HitResult;
            __result = scp096HitResult;
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error("Scp096Attack Patch failed\n" + e);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp049Attack", PatchType.ScpEvent)]
public static class Scp049AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp049AttackPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp049AttackAbility), nameof(Scp049AttackAbility.ServerProcessCmd))]
    public static bool ServerProcessCmd(Scp049AttackAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady || __instance._resurrect.IsInProgress)
            {
                return false;
            }

            __instance._target = reader.ReadReferenceHub();
            if (__instance._target == null || !__instance.IsTargetValid(__instance._target)) return false;
            var effect = __instance._target.playerEffectsController.GetEffect<CardiacArrest>();

            var scp = __instance.Owner.GetSynapsePlayer();
            var victim = __instance._target.GetSynapsePlayer();
            var damage = 0f;
            var enableCardiac = true;
            if (effect.IsEnabled)
            {
                enableCardiac = false;
                damage = -1;
            }

            var ev = new Scp049AttackEvent(scp, victim, damage, 1.5f, enableCardiac);
            _scp.Scp049Attack.RaiseSafely(ev);
            if (!ev.Allow) return false;

            if (ev.Cooldown > 0f)
                __instance.Cooldown.Trigger(ev.Cooldown);

            if (ev.Damage != 0)
            {
                __instance._target.playerStats.DealDamage(new Scp049DamageHandler(__instance.Owner, ev.Damage,
                    Scp049DamageHandler.AttackType.Instakill));
            }

            if (ev.EnableCardiacEffect && victim.Hub.IsAlive())
            {
                effect.SetAttacker(__instance.Owner);
                effect.Intensity = 1;
                effect.ServerChangeDuration(__instance._statusEffectDuration);
            }

            Synapse3Extensions.RaiseEvent(__instance, nameof(Scp049AttackAbility.OnServerHit), __instance._target);
            __instance.ServerSendRpc(true);
            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error("Scp049Attack Patch failed\n" + e);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp106Attack", PatchType.ScpEvent)]
public static class Scp106AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp106AttackPatch() => _scp = Synapse.Get<ScpEvents>();


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp106Attack), nameof(Scp106Attack.ServerShoot))]
    public static bool OnServerShoot(Scp106Attack __instance)
    {
        try
        {
            Scp106AttackEvent ev;
            var scp = __instance.Owner.GetSynapsePlayer();
            var victim = __instance._targetHub.GetSynapsePlayer();

            using (new FpcBacktracker(scp, __instance._targetPosition, 0.35f))
            {
                var vector = __instance._targetPosition - __instance._ownerPosition;
                var sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude > __instance._maxRangeSqr)
                    return false;

                var forward = __instance.OwnerCam.forward;
                forward.y = 0f;
                vector.y = 0f;
                if (Physics.Linecast(__instance._ownerPosition, __instance._targetPosition, MicroHIDItem.WallMask))
                    return false;

                if (__instance._dotOverDistance.Evaluate(sqrMagnitude) >
                    Vector3.Dot(vector.normalized, forward.normalized))
                {
                    ev = new Scp106AttackEvent(scp, null, 0, false, __instance._missCooldown);
                    _scp.Scp106Attack.RaiseSafely(ev);
                    if (!ev.Allow) return false;
                    __instance.SendCooldown(ev.Cooldown);
                    return false;
                }

                ev = new Scp106AttackEvent(scp, victim, __instance._damage, true, __instance._hitCooldown);
                _scp.Scp106Attack.RaiseSafely(ev);

                if (!EventManager.ExecuteEvent(ServerEventType.Scp106TeleportPlayer, __instance.Owner,
                        __instance._targetHub))
                    return false;

                if (!ev.Allow) return false;

                var handler = new ScpDamageHandler(__instance.Owner, ev.Damage, DeathTranslations.PocketDecay);
                if (!__instance._targetHub.playerStats.DealDamage(handler))
                {
                    return false;
                }
            }

            __instance.SendCooldown(ev.Cooldown);
            __instance.Vigor.VigorAmount += 0.3f;
            __instance.ReduceSinkholeCooldown();
            Hitmarker.SendHitmarker(scp, 1f);
            if (!ev.TakeToPocket) return false;

            Synapse3Extensions.RaiseEvent(typeof(Scp106Attack), nameof(Scp106Attack.OnPlayerTeleported), victim.Hub);
            var playerEffectsController = victim.PlayerEffectsController;
            playerEffectsController.EnableEffect<Traumatized>(180f);
            playerEffectsController.EnableEffect<Corroding>();
            scp.MainScpController.Scp106.PlayersInPocket.Add(ev.Victim);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp106Attack Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173AttackSnap", PatchType.ScpEvent)]
public static class Scp173AttackSnapPatch
{
    private static readonly ScpEvents _scp;
    static Scp173AttackSnapPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173SnapAbility), nameof(Scp173SnapAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173SnapAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._targetHub = reader.ReadReferenceHub();
            if (__instance._observersTracker.IsObserved
                || __instance._targetHub == null
                || __instance._targetHub.roleManager.CurrentRole is not IFpcRole currentRole
                || __instance.IsSpeeding)
            {
                return false;
            }

            var scpCameraReference = __instance.Owner.PlayerCameraReference;

            var scpFpcModule = __instance.ScpRole.FpcModule;
            var victimFpcModule = currentRole.FpcModule;

            var victimPosition = victimFpcModule.Position;
            var scpPosition = scpFpcModule.Position;

            var scpRotation = scpCameraReference.rotation;

            victimFpcModule.Position = victimFpcModule.Tracer.GenerateBounds(0.4f, true)
                .ClosestPoint(reader.ReadRelativePosition().Position);
            var bounds = scpFpcModule.Tracer.GenerateBounds(0.1f, true);
            bounds.Encapsulate(scpFpcModule.Position + scpFpcModule.Motor.Velocity * 0.2f);
            scpFpcModule.Position = bounds.ClosestPoint(reader.ReadRelativePosition().Position);
            scpCameraReference.rotation = reader.ReadLowPrecisionQuaternion().Value;

            if (Scp173SnapAbility.TryHitTarget(scpCameraReference, out var target))
            {
                var scp = __instance.Owner.GetSynapsePlayer();
                var ev = new Scp173AttackEvent(scp, target.GetSynapsePlayer(), -1, false);
                ev.Allow = ev.Allow && EventManager.ExecuteEvent(ServerEventType.Scp173SnapPlayer, __instance.Owner,
                    __instance._targetHub);
                _scp.Scp173Attack.RaiseSafely(ev);
                var damageHandler = new ScpDamageHandler(scp, ev.Damage, DeathTranslations.Scp173);

                if (ev.Allow)
                {
                    if (target.playerStats.DealDamage(damageHandler))
                    {
                        Hitmarker.SendHitmarker(__instance.Owner, 1f);
                        if (__instance.ScpRole.SubroutineModule.TryGetSubroutine<Scp173AudioPlayer>(out var subroutine))
                        {
                            subroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);
                        }
                    }
                }
            }

            victimFpcModule.Position = victimPosition;
            scpFpcModule.Position = scpPosition;
            scpCameraReference.rotation = scpRotation;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp173AttackSnap Patch failed\n" + ex);
            //Depending on where the error was thrown is the Network reader already partially read so that the base game would throw an error and kick SCP-173
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173AttackTp", PatchType.ScpEvent)]
public static class Scp173AttackTpPatch
{
    private static readonly ScpEvents _scp;
    static Scp173AttackTpPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173TeleportAbility), nameof(Scp173TeleportAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173TeleportAbility __instance, NetworkReader reader)
    {
        try
        {
            __instance._cmdData = (CmdTeleportData)reader.ReadByte();
            if (!__instance.HasDataFlag(CmdTeleportData.WantsToTeleport))
            {
                __instance.ServerSendRpc(true);
                return false;
            }

            if (!__instance._blinkTimer.AbilityReady)
                return false;

            var playerCameraReference = __instance.Owner.PlayerCameraReference;
            var prevObservers = new HashSet<ReferenceHub>(__instance._observersTracker.Observers);
            var cmdData = __instance._cmdData;
            __instance._cmdData = 0;
            __instance.ServerSendRpc(true);
            __instance._cmdData = cmdData;
            var rotation = playerCameraReference.rotation;
            playerCameraReference.rotation = reader.ReadQuaternion();
            var canBlink = __instance.TryBlink(reader.ReadFloat());
            playerCameraReference.rotation = rotation;
            if (!canBlink)
                return false;

            prevObservers.UnionWith(__instance._observersTracker.Observers);
            __instance.ServerSendRpc(x => prevObservers.Contains(x));
            __instance._audioSubroutine.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Teleport);
            if (__instance._breakneckSpeedsAbility.IsActive)
                return false;

            var objectCount =
                Physics.OverlapSphereNonAlloc(__instance._fpcModule.Position, 0.8f, DetectedColliders, 16384);

            for (var i = 0; i < objectCount; i++)
            {
                if (DetectedColliders[i].TryGetComponent<BreakableWindow>(out var component))
                {
                    component.Damage(component.health, __instance.ScpRole.DamageHandler, Vector3.zero);
                }
            }

            var targetHub = reader.ReadReferenceHub();

            if (targetHub == null || targetHub.roleManager.CurrentRole is not FpcStandardRoleBase fpcRole) return false;


            var bounds = fpcRole.FpcModule.Tracer.GenerateBounds(0.4f, true);
            bounds.Encapsulate(new Bounds(fpcRole.FpcModule.Position, Vector3.up * 2.2f));
            if (bounds.SqrDistance(__instance._fpcModule.Position) > 1.66f)
                return false;

            var scp = __instance.Owner.GetSynapsePlayer();
            var ev = new Scp173AttackEvent(scp, targetHub.GetSynapsePlayer(), -1, true);
            ev.Allow = ev.Allow &&
                       EventManager.ExecuteEvent(ServerEventType.Scp173SnapPlayer, __instance.Owner, targetHub);
            _scp.Scp173Attack.RaiseSafely(ev);
            if (!ev.Allow) return false;

            if (!targetHub.playerStats.DealDamage(new ScpDamageHandler(scp, ev.Damage, DeathTranslations.Scp173)))
                return false;

            if (!__instance.ScpRole.SubroutineModule.TryGetSubroutine<Scp173AudioPlayer>(out var audioPlayer))
                return false;

            Hitmarker.SendHitmarker(__instance.Owner, 1f);
            audioPlayer.ServerSendSound(Scp173AudioPlayer.Scp173SoundId.Snap);

            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp173AttackTp Patch failed\n" + ex);
            //Depending on where the error was thrown is the Network reader already partially read so that the base game would throw an error and kick SCP-173
            return false;
        }
    }
}

[Automatic]
[SynapsePatch("Scp939Damage", PatchType.ScpEvent)]
public static class Scp939DamagePatch
{
    private static readonly ScpEvents _scp;
    static Scp939DamagePatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp939DamageHandler), nameof(Scp939DamageHandler.ProcessDamage))]
    public static bool OnProcessDamage(Scp939DamageHandler __instance, ReferenceHub ply)
    {
        try
        {
            var scp = __instance.Attacker.GetSynapsePlayer();
            var victim = ply.GetSynapsePlayer();
            var ev = new Scp939AttackEvent(scp, victim, __instance.Damage, __instance._damageType);
            _scp.Scp939Attack.RaiseSafely(ev);
            __instance.Damage = ev.Damage;
            if (!ev.Allow) __instance.Damage = 0;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp939Attack Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp0492Attack", PatchType.ScpEvent)]
public static class Scp0492AttackPatch
{
    private static readonly ScpEvents _scp;
    static Scp0492AttackPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.ProcessDamage))]
    public static bool OnProcessDamage(AttackerDamageHandler __instance, ReferenceHub ply)
    {
        try
        {
            if (__instance is not Scp049DamageHandler
                {
                    DamageSubType: Scp049DamageHandler.AttackType.Scp0492
                }) return true;
            var scp = __instance.Attacker.GetSynapsePlayer();
            var victim = ply.GetSynapsePlayer();
            var ev = new Scp0492AttackEvent(scp, victim, __instance.Damage);
            _scp.Scp0492Attack.RaiseSafely(ev);
            __instance.Damage = ev.Damage;
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp0492 Attack Patch failed\n" + ex);
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("PlayerEscapePocketDimension", PatchType.ScpEvent)]
public static class PlayerEscapePocketDimensionPatch
{
    private static readonly ScpEvents Scp;
    static PlayerEscapePocketDimensionPatch() => Scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static bool OnTriggerEnter(PocketDimensionTeleport __instance, Collider other)
    {
        try
        {
            var component = other.GetComponent<NetworkIdentity>();
            if (component == null
                || !ReferenceHub.TryGetHubNetID(component.netId, out var hub)
                || hub.roleManager.CurrentRole.ActiveTime < 1f)
                return false;

            var player = hub.GetSynapsePlayer();
            if (player.CurrentRole is not IFpcRole fpcRole) return false;
            var escapePosition = Scp106PocketExitFinder.GetBestExitPosition(fpcRole);
            var enteredPosition = __instance.transform.position;

            var escape = __instance._type == PDTeleportType.Exit || AlphaWarheadController.Detonated;
            var ev = new Scp106LeavePocketEvent(player, escape, enteredPosition, escapePosition);

            Scp.Scp106LeavePocket.RaiseSafely(ev);

            if (!ev.Allow) return false;

            if (!ev.EscapePocket)
            {
                if (!EventManager.ExecuteEvent(ServerEventType.PlayerExitPocketDimension, hub, false))
                    return false;
                hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.PocketDecay));
            }
            else
            {
                if (!EventManager.ExecuteEvent(ServerEventType.PlayerExitPocketDimension, hub, true))
                    return false;
                fpcRole.FpcModule.ServerOverridePosition(ev.EscapePosition, Vector3.zero);
                hub.playerEffectsController.EnableEffect<Disabled>(10f, addDuration: true);
                hub.playerEffectsController.DisableEffect<Corroding>();
                ImageGenerator.pocketDimensionGenerator.GenerateRandom();
            }

            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"PlayerEscapePocketDimension Patch failed\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173Tantrum", PatchType.ScpEvent)]
public static class Scp173TantrumPatch
{
    private static readonly ScpEvents _scp;
    static Scp173TantrumPatch() => _scp = Synapse.Get<ScpEvents>();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173TantrumAbility), nameof(Scp173TantrumAbility.ServerProcessCmd))]
    public static bool OnServerProcessCmd(Scp173TantrumAbility __instance, NetworkReader reader)
    {
        try
        {
            if (!__instance.Cooldown.IsReady
                || __instance._observersTracker.IsObserved
                || !Physics.Raycast(__instance.ScpRole.FpcModule.Position, Vector3.down, out var hitInfo, 3f,
                    __instance._tantrumMask))
                return false;

            var player = __instance.ScpRole._owner.GetSynapsePlayer();

            var ev = new Scp173PlaceTantrumEvent(player, player.MainScpController.Scp173.TantrumCoolDown)
            {
                Allow = EventManager.ExecuteEvent(ServerEventType.Scp173CreateTantrum, __instance.Owner)
            };

            _scp.Scp173PlaceTantrum.RaiseSafely(ev);

            if (!ev.Allow) return false;

            __instance.Cooldown.Trigger(ev.CoolDown);
            __instance.ServerSendRpc(true);
            var tantrumEnvironmentalHazard = UnityEngine.Object.Instantiate(__instance._tantrumPrefab);
            var targetPos = hitInfo.point + Vector3.up * 1.25f;
            tantrumEnvironmentalHazard.SynchronizedPosition = new RelativePosition(targetPos);
            NetworkServer.Spawn(tantrumEnvironmentalHazard.gameObject);
            foreach (TeslaGate teslaGate in TeslaGateController.Singleton.TeslaGates)
            {
                if (teslaGate.PlayerInIdleRange(__instance.Owner))
                {
                    teslaGate.TantrumsToBeDestroyed.Add(tantrumEnvironmentalHazard);
                }
            }

            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Scp173Tantrum Patch failed\n{e}");
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Scp173ObserveEvent", PatchType.ScpEvent)]
public static class Scp173ObserversListPatch
{
    private static readonly ScpEvents _scp;
    private static readonly SynapseConfigService _config;

    static Scp173ObserversListPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
        _config = Synapse.Get<SynapseConfigService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.CheckRemovedPlayer))]
    public static bool CheckRemovedPlayer(Scp173ObserversTracker __instance, ReferenceHub ply)
    {
        try
        {
            if (__instance == null || __instance.Owner == null) return true;
            var player = __instance.Owner.GetSynapsePlayer();
            if (player == null) return true;
            var controller = player.MainScpController.Scp173;

            if (!__instance.Observers.Remove(ply)) return false;
            controller._observer.Remove(player);
            __instance.CurrentObservers--;
            return false;
        }
        catch (Exception e)
        {
            SynapseLogger<Synapse>.Error($"Scp173 Check RemovedPlayers Patch failed\n{e}");
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))]
    public static bool UpdateObserver(Scp173ObserversTracker __instance, out int __result, ReferenceHub targetHub)
    {
        try
        {
            if (targetHub == null || __instance.Owner == targetHub)
            {
                __result = 0;
                return false;
            }

            var player = targetHub.GetSynapsePlayer();
            var scp = __instance.GetSynapsePlayer();
            var controller = scp.MainScpController.Scp173;

            if (player.Hub.IsAlive() && !_config.GamePlayConfiguration.CantObserve173.Contains(player.RoleID) &&
                player.PlayerType != PlayerType.Server)
            {
                if (__instance.IsObservedBy(targetHub, 0.2f))
                {
                    var ev = new Scp173ObserveEvent(player, player.Invisible < InvisibleMode.Ghost, scp);
                    _scp.Scp173Observe.RaiseSafely(ev);

                    if (!ev.Allow)
                    {
                        if (__instance.Observers.Remove(targetHub))
                            goto RemovePlayer;

                        __result = 0;
                        return false;
                    }

                    if (__instance.Observers.Add(targetHub))
                        goto AddPlayer;
                }
                else if (__instance.Observers.Remove(targetHub))
                    goto RemovePlayer;

                __result = 0;
                return false;
            }

            if (__instance.Observers.Remove(targetHub))
                goto RemovePlayer;

            __result = 0;
            return false;


            RemovePlayer:
            controller._observer.Remove(player);
            __result = -1;
            return false;

            AddPlayer:
            controller._observer.Add(player);
            __result = 1;
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp173 Update Observers Patch failed\n" + ex);
            __result = 0;
            return true;
        }
    }
}

[Automatic]
[SynapsePatch("Add096Target", PatchType.ScpEvent)]
public static class Add096TargetPatch
{
    private static readonly ScpEvents _scp;
    private static readonly SynapseConfigService _config;

    static Add096TargetPatch()
    {
        _scp = Synapse.Get<ScpEvents>();
        _config = Synapse.Get<SynapseConfigService>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096TargetsTracker), nameof(Scp096TargetsTracker.AddTarget))]
    public static bool Add096Target(Scp096TargetsTracker __instance, ReferenceHub target, bool isForLook)
    {
        try
        {
            if (__instance.Targets.Contains(target)) return false;
            var player = target.GetSynapsePlayer();
            var scp = __instance.GetSynapsePlayer();
            if (player == null || scp == null ||
                (_config.GamePlayConfiguration.CantObserve096?.Contains(player.RoleID) ?? false)) return false;
            var ev = new Scp096AddTargetEvent(player, player.Invisible < InvisibleMode.Ghost, scp, isForLook);
            _scp.Scp096AddTarget.RaiseSafely(ev);
            return ev.Allow;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp173 Update Observers Patch failed\n" + ex);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scp096TargetsTracker), nameof(Scp096TargetsTracker.UpdateTarget))]
    public static bool UpdateTarget(Scp096TargetsTracker __instance, ReferenceHub target)
    {
        try
        {
            var player = target.GetSynapsePlayer();
            if (_config.GamePlayConfiguration.CantObserve096?.Contains(player?.RoleID ?? RoleService.NoneRole) ?? false)
            {
                __instance.RemoveTarget(target);
                return false;
            }

            if (!__instance.IsObservedBy(target))
            {
                return false;
            }

            __instance.AddTarget(target, true);
            return false;
        }
        catch (Exception ex)
        {
            SynapseLogger<Synapse>.Error("Scp173 Update Observers Patch failed\n" + ex);
            return true;
        }
    }
}
#endif