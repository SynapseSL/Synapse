using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Interactables.Interobjects;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace Synapse3.SynapseModule.Map.Elevators;

public class ElevatorService : Service
{
    private readonly Synapse _synapseModule;
    private readonly RoundEvents _round;
    private readonly MapEvents _map;
    private readonly PlayerEvents _playerEvents;
    private readonly PlayerService _playerService;

    public ElevatorService(Synapse synapseModule, RoundEvents round, MapEvents map,PlayerEvents playerEvents, PlayerService playerService)
    {
        _synapseModule = synapseModule;
        _round = round;
        _map = map;
        _playerEvents = playerEvents;
        _playerService = playerService;
    }

    public override void Enable()
    {
        while (_synapseModule.ModuleElevatorBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleElevatorBindingQueue.Dequeue();
            LoadBinding(binding);
        }
        
        _round.Waiting.Subscribe(OnWaiting);
        _round.Restart.Subscribe(Clear);
        _map.ElevatorMoveContent.Subscribe(MoveContent);
        _playerEvents.Update.Subscribe(Update);
        ElevatorChamber.OnElevatorMoved += MoveVanillaContent;
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(OnWaiting);
        _round.Restart.Unsubscribe(Clear);
        _map.ElevatorMoveContent.Unsubscribe(MoveContent);
        _playerEvents.Update.Unsubscribe(Update);
        ElevatorChamber.OnElevatorMoved -= MoveVanillaContent;
    }

    internal readonly List<IElevator> _elevators = new ();
    public ReadOnlyCollection<IElevator> Elevators => _elevators.AsReadOnly();
    
    internal readonly List<ICustomElevator> _customElevators = new();

    public ReadOnlyCollection<ICustomElevator> CustomElevators => _customElevators.AsReadOnly();

    internal void LoadBinding(SynapseElevatorBinding binding) => RegisterElevator(binding.Type, binding.Info);

    public void RegisterElevator(Type elevatorType, ElevatorAttribute info)
    {
        if(IsIdRegistered(info.Id)) return;
        if (!typeof(ICustomElevator).IsAssignableFrom(elevatorType)) return;

        var customElevator = (ICustomElevator)Synapse.GetOrCreate(elevatorType);
        customElevator.Attribute = info;
        customElevator.Load();
        
        _customElevators.Add(customElevator);
        _elevators.Add(customElevator);
    }

    public bool IsIdRegistered(uint id) => _elevators.Any(x => x.ElevatorId == id);
    
    private void OnWaiting(RoundWaitingEvent _)
    {
        _elevators.Clear();
        foreach (var elevator in ElevatorManager.SpawnedChambers)
        {
            _elevators.Add(new SynapseElevator(elevator.Value));
        }

        foreach (var elevator in _customElevators)
        {
            elevator.Generate();
            _elevators.Add(elevator);
        }
    }

    private void MoveContent(ElevatorMoveContentEvent ev)
    {
        if (!ev.CustomElevator) return;
        foreach (var player in _playerService.Players)
        {
            if (!ev.Bounds.Contains(player.Position)) continue;
            Logger.Warn(ev.DeltaPosition);
            player.Position += ev.DeltaPosition;
        }
    }

    private void Clear(RoundRestartEvent _)
    {
        _elevators.Clear();
        foreach (var elevator in _customElevators)
        {
            elevator.Unload();
        }
    }

    private void Update(UpdateEvent ev)
    {
        if (ev.Player.PlayerType != PlayerType.Server) return;
        foreach (var customElevator in _customElevators)
        {
            if(customElevator.Chamber is ICustomElevatorChamber customElevatorChamber)
                customElevatorChamber.Update();
        }
    }

    private void MoveVanillaContent(Bounds elevatorBounds, ElevatorChamber chamber, Vector3 deltaPos,
        Quaternion deltaRot)
    {
        _map.ElevatorMoveContent.RaiseSafely(new ElevatorMoveContentEvent(chamber.GetSynapseElevator(), deltaPos,
            deltaRot, elevatorBounds));
    }
}