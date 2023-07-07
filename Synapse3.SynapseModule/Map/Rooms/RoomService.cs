using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using MapGeneration;
using Mono.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using static Synapse3.SynapseModule.Map.MapService;
using static Synapse3.SynapseModule.Map.Rooms.RoomService;

namespace Synapse3.SynapseModule.Map.Rooms;

public class RoomService : Service
{
    public const int HighestRoom = (int)RoomType.GateB;

    private readonly RoundEvents _round;
    private readonly MapService _map;
    private readonly Synapse _synapseModule;
    private readonly PlayerEvents _player;

    internal readonly Dictionary<SynapsePlayer, PlayerUpdateRoom> PlayerUpdateRooms = new();

    public RoomService(RoundEvents round, MapService map, Synapse synapseModule, PlayerEvents player)
    {
        _round = round;
        _map = map;
        _synapseModule = synapseModule;
        _player = player;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(LoadRooms);
        _round.Restart.Subscribe(ClearRooms);
        _player.Update.Subscribe(OnUpdate);

        while (_synapseModule.ModuleRoomBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleRoomBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(LoadRooms);
        _round.Restart.Unsubscribe(ClearRooms);
        _player.Update.Unsubscribe(OnUpdate);
    }

    internal void LoadBinding(SynapseRoomBinding binding) => RegisterCustomRoom(binding.Info);

    internal readonly List<IRoom> _rooms = new();
    internal readonly List<SynapseCustomRoom> _customRooms = new();
    private readonly List<CustomRoomAttribute> _customRoomInformation = new();


    public ReadOnlyCollection<IRoom> Rooms => _rooms.AsReadOnly();
    public ReadOnlyCollection<SynapseCustomRoom> CustomRooms => _customRooms.AsReadOnly();
    public ReadOnlyCollection<CustomRoomAttribute> CustomRoomInformation => _customRoomInformation.AsReadOnly();


    public IRoom GetRoom(uint id)
        => Rooms.FirstOrDefault(x => x.Id == id);

    public IRoom GetRoom(string name)
        => Rooms.FirstOrDefault(x => x.Name == name);

    public IRoom GetNearestRoom(Vector3 position)
    {
        var room = RoomIdUtils.RoomAtPosition(position);

        return room != null ? room.GetVanillaRoom() : Rooms.OrderBy(x => Vector3.Distance(x.Position, position))?.FirstOrDefault();
    }

    public void AddRoom(IRoom room)
    {
        if (!_rooms.Contains(room))
            _rooms.Add(room);
    }

    public void RegisterCustomRoom<TRoom>() where TRoom : SynapseCustomRoom
    {
        var info = typeof(TRoom).GetCustomAttribute<CustomRoomAttribute>();
        if (info == null) return;
        info.RoomType = typeof(TRoom);

        RegisterCustomRoom(info);
    }

    public void RegisterCustomRoom(CustomRoomAttribute info)
    {
        if (info.RoomType == null) return;
        if (IsIdRegistered(info.Id)) return;

        _customRoomInformation.Add(info);
    }

    public void UnRegisterCustomRoom(uint id)
    {
        if (!IsIdRegistered(id)) return;
        var info = _customRoomInformation.First(x => x.Id == id);
        _customRoomInformation.Remove(info);
    }

    public SynapseCustomRoom CreateRoom(uint id)
    {
        if (!IsIdRegistered(id)) return null;
        var info = _customRoomInformation.FirstOrDefault(x => x.Id == id);
        return info == null ? null : CreateRoom(info);
    }

    public SynapseCustomRoom CreateRoom(CustomRoomAttribute info)
    {
        if (info.RoomType == null) return null;
        var room = (SynapseCustomRoom)Synapse.Create(info.RoomType, false);
        room.Attribute = info;
        room.Load();
        return room;
    }

    public SynapseCustomRoom SpawnCustomRoom(uint id, Vector3 position)
    {
        if (!IsIdRegistered(id) || id is >= 0 and <= HighestRoom) return null;
        var room = CreateRoom(id);
        room.Generate(position);
        return room;
    }

    public bool IsIdRegistered(uint id)
        => id is >= 0 and <= HighestRoom || _customRoomInformation.Any(x => x.Id == id);

    private void OnUpdate(UpdateEvent ev)
    {
        if (!PlayerUpdateRooms.TryGetValue(ev.Player, out var playerUpdate))
            playerUpdate = PlayerUpdateRooms[ev.Player] = new PlayerUpdateRoom(ev.Player);

        foreach (var room in _customRooms)
        {
            playerUpdate.Update(room);
        }

    }

    private void LoadRooms(RoundWaitingEvent ev)
    {
        foreach (var room in RoomIdentifier.AllRoomIdentifiers)
        {
            var type = GetRoomTypeFromName(room.gameObject.name);

            IVanillaRoom iRoom;
            switch (type)
            {
                case RoomType.TestingRoom:
                case RoomType.Scp330:
                    iRoom = new SynapseNetworkRoom(room, type);
                    break;

                default:
                    iRoom = new SynapseRoom(room, type);
                    break;

            }

            _rooms.Add(iRoom);
            _map._synapseCameras.AddRange(iRoom.Cameras);
        }
    }

    private void ClearRooms(RoundRestartEvent ev)
    {
        _rooms.Clear();
        _customRooms.Clear();
        SynapseNetworkRoom._networkIdentities.Clear();
    }

    public RoomType GetRoomTypeFromName(string roomName)
    {
        var nameLower = roomName.ToLower();

        foreach (var pair in RoomByNames)
        {
            if (nameLower.Contains(pair.Key.ToLower())) return pair.Value;
        }

        return RoomType.None;
    }

    public readonly ReadOnlyDictionary<string, RoomType> RoomByNames = new(
        new Dictionary<string, RoomType>
        {
            { "PocketWorld", RoomType.Pocket },
            { "Outside", RoomType.Surface },

            { "LCZ_Straight", RoomType.LczStraight },
            { "LCZ_Airlock", RoomType.LczAirlock },
            { "LCZ_Curve", RoomType.LczCurve },
            { "LCZ_TCross", RoomType.LczTCross },
            { "LCZ_Crossing", RoomType.LczCrossing },
            { "LCZ_ClassDSpawn", RoomType.ClassDSpawn },
            { "LCZ_Cafe", RoomType.LczOffice },
            { "LCZ_Toilets", RoomType.Toilets },
            { "LCZ_Armory", RoomType.LightArmory },
            { "LCZ_Plants", RoomType.Plants },
            { "LCZ_ChkpA", RoomType.LczCheckpointA },
            { "LCZ_ChkpB", RoomType.LczCheckpointB },
            { "LCZ_330", RoomType.Scp330 },
            { "LCZ_173", RoomType.Scp173 },
            { "LCZ_372", RoomType.Scp372 },
            { "LCZ_914", RoomType.Scp914 },

            { "HCZ_Straight", RoomType.HczStraight },
            { "HCZ_Curve", RoomType.HczCurve },
            { "HCZ_Crossing", RoomType.HczCrossing },
            { "HCZ_Room3ar", RoomType.HeavyArmory },
            { "HCZ_Testroom", RoomType.TestingRoom },
            { "HCZ_Room3", RoomType.HczTCross },
            { "HCZ_Nuke", RoomType.Nuke },
            { "HCZ_Servers", RoomType.Servers },
            { "HCZ_Tesla", RoomType.Tesla },
            { "HCZ_Hid", RoomType.MicroHid },
            { "HCZ_ChkpA", RoomType.HczElevatorA },
            { "HCZ_ChkpB", RoomType.HczElevatorB },
            { "HCZ Part", RoomType.HeavyCheckpoint },
            { "HCZ_049", RoomType.Scp049 },
            { "HCZ_079", RoomType.Scp079 },
            { "HCZ_457", RoomType.Scp096 },
            { "HCZ_106", RoomType.Scp106 },
            { "HCZ_939", RoomType.Scp939 },

            { "EZ Part", RoomType.EntranceCheckpoint },
            { "EZ_Straight", RoomType.EzStraight },
            { "EZ_Curve", RoomType.EzCurve },
            { "EZ_ThreeWay", RoomType.EzTCross },
            { "EZ_Crossing", RoomType.EzCrossing },
            { "EZ_Endoof", RoomType.EzDeadEnd },
            { "EZ_CollapsedTunnel", RoomType.CollapsedTunnel },
            { "EZ_Intercom", RoomType.Intercom },
            { "EZ_PCs_small", RoomType.EzOfficeDownstairs },
            //I don't know why but one Straight EZ Room is called Cafeteria for no Reason
            { "EZ_Cafeteria", RoomType.EzStraight },
            { "EZ_PCs", RoomType.EzOffice },
            { "EZ_Smallrooms2", RoomType.ConferenceRoomCorridor },
            { "EZ_Shelter", RoomType.Shelter },
            { "EZ_upstairs", RoomType.EzOfficeUpstairs },
            { "EZ_GateA", RoomType.GateA },
            { "EZ_GateB", RoomType.GateB }
        });

    internal class PlayerUpdateRoom
    {
        public readonly SynapsePlayer player;

        public readonly Dictionary<SynapseCustomRoom, bool> roomsVisble = new();

        public readonly Dictionary<SynapseCustomRoom, float> roomsNextUpdate = new();

        public PlayerUpdateRoom(SynapsePlayer player)
        {
            this.player = player;
        }

        public void Update(SynapseCustomRoom room)
        {
            if (room.VisibleDistance <= 0) return;

            if (!roomsNextUpdate.TryGetValue(room, out var nextUpdate))
                roomsNextUpdate[room] = nextUpdate = 0;

            if (room.UpdateFrequencyVisble > 0 && Time.time < nextUpdate) return;
            
            roomsNextUpdate[room] = Time.time + room.UpdateFrequencyVisble;

            if (!roomsVisble.TryGetValue(room, out var visible))
                visible = roomsVisble[room] = true;

            if (Vector3.Distance(player.Position, room.Position) >= room.VisibleDistance)
            {
                if (!visible) return;
                room.HideFromPlayer(player);
                roomsVisble[room] = false;
            }
            else
            {
                if (visible) return;
                room.ShowPlayer(player);
                roomsVisble[room] = true;
            }
        }
    }
}