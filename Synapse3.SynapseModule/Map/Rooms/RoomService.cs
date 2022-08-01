using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using MapGeneration;
using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public class RoomService : Service
{
    public const int HighestRoom = (int)RoomType.GateB;
    
    private readonly RoundEvents _round;
    private readonly MapService _map;
    private readonly IKernel _kernel;
    private readonly Synapse _synapseModule;

    public RoomService(RoundEvents round, MapService map, IKernel kernel, Synapse synapseModule)
    {
        _round = round;
        _map = map;
        _kernel = kernel;
        _synapseModule = synapseModule;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(LoadRooms);
        _round.Restart.Subscribe(ClearRooms);
        
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
    }

    internal void LoadBinding(SynapseRoomBinding binding) => RegisterCustomRoom(binding.Info);

    internal readonly List<IRoom> _rooms = new();
    private readonly List<CustomRoomAttribute> _customRoomInformation = new ();
    
    
    public ReadOnlyCollection<IRoom> Rooms => _rooms.AsReadOnly();
    public ReadOnlyCollection<CustomRoomAttribute> CustomRoomInformation => _customRoomInformation.AsReadOnly();

    
    public IRoom GetRoom(int id)
        => Rooms.FirstOrDefault(x => x.Id == id);

    public IRoom GetRoom(string name)
        => Rooms.FirstOrDefault(x => x.Name == name);

    //TODO: Find a better solution maybe Raycasts
    public IRoom GetNearestRoom(Vector3 position)
    {
        return Rooms.OrderBy(x => Vector3.Distance(x.Position, position))?.FirstOrDefault();
    }

    public void AddRoom(IRoom room)
    {
        if (!_rooms.Contains(room))
            _rooms.Add(room);
    }

    public void RegisterCustomRoom<TRoom>() where TRoom : SynapseCustomRoom
    {
        var info = typeof(TRoom).GetCustomAttribute<CustomRoomAttribute>();
        if(info == null) return;
        info.RoomType = typeof(TRoom);
        
        RegisterCustomRoom(info);
    }
    
    public void RegisterCustomRoom(CustomRoomAttribute info)
    {
        if(info.RoomType == null) return;
        if(IsIdRegistered(info.Id)) return;
        
        _customRoomInformation.Add(info);
    }

    public void UnRegisterCustomRoom(int id)
    {
        if(!IsIdRegistered(id)) return;
        var info = _customRoomInformation.First(x => x.Id == id);
        _customRoomInformation.Remove(info);
    }

    public SynapseCustomRoom CreateRoom(int id)
    {
        if (!IsIdRegistered(id)) return null;
        var info = _customRoomInformation.FirstOrDefault(x => x.Id == id);
        if (info == null) return null;

        return CreateRoom(info);
    }

    public SynapseCustomRoom CreateRoom(CustomRoomAttribute info)
    {
        if (info.RoomType == null) return null;
        var room = (SynapseCustomRoom)_kernel.Get(info.RoomType);
        room.Attribute = info;
        room.Load();
        return room;
    }

    public SynapseCustomRoom SpawnCustomRoom(int id, Vector3 position)
    {
        if (!IsIdRegistered(id) || id is >= 0 and <= HighestRoom) return null;
        var room = CreateRoom(id);
        room.Generate(position);
        return room;
    }

    public bool IsIdRegistered(int id)
        => id is >= 0 and <= HighestRoom || _customRoomInformation.Any(x => x.Id == id);
    
    private void LoadRooms(RoundWaitingEvent ev)
    {
        foreach (var room in RoomIdentifier.AllRoomIdentifiers)
        {
            var type = GetRoomTypeFromName(room.gameObject.name);

            IVanillaRoom iRoom;
            switch (type)
            {
                case RoomType.Scp939:
                    case RoomType.Scp330:
                    case RoomType.Scp106:
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
        SynapseNetworkRoom._networkIdentities.Clear();
    }

    public RoomType GetRoomTypeFromName(string roomName)
    {
        foreach (var pair in RoomTypes)
        {
            if (roomName.ToLower().Contains(pair.Key.ToLower())) return pair.Value;
        }

        return RoomType.None;
    }

    public readonly ReadOnlyDictionary<string, RoomType> RoomTypes = new(
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
            { "HCZ_Testroom", RoomType.Scp939 },
            { "HCZ_Room3", RoomType.HczTCross },
            { "HCZ_Nuke", RoomType.Nuke },
            { "HCZ_Servers", RoomType.Servers },
            { "HCZ_Tesla", RoomType.Tesla },
            { "HCZ_Hid", RoomType.MicroHid },
            { "HCZ_ChkpA", RoomType.HczCheckpointA },
            { "HCZ_ChkpB", RoomType.HczCheckpointB },
            { "HCZ_EZ_Checkpoint", RoomType.HeavyEntranceCheckpoint },
            { "HCZ_049", RoomType.Scp049 },
            { "HCZ_079", RoomType.Scp079 },
            { "HCZ_457", RoomType.Scp096 },
            { "HCZ_106", RoomType.Scp106 },

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
}