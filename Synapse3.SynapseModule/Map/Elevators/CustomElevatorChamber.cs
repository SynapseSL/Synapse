using Ninject;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class CustomElevatorChamber : ICustomElevatorChamber
{
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _elapsedTime = 0f;
    private float _moveTime = 0f;
    
    [Inject]
    public SchematicService SchematicService { get; set; }
    
    [Inject]
    public MapEvents MapEvents { get; set; }
    
    public SynapseSchematic Schematic { get; private set; }
    public Transform ParentTransform => Schematic.GameObject.transform;
    public Vector3 Position => Schematic.Position;
    public Quaternion Rotation => Schematic.Rotation;
    public bool IsMoving { get; private set; } = false;
    public IElevator MainElevator { get; private set; }
    public void Update()
    {
        if (!IsMoving) return;
        var currentRot = Rotation.eulerAngles;
        var deltaPos = _targetPosition - Position;
        var deltaRotRaw = _targetRotation.eulerAngles - currentRot;
        if (_elapsedTime < _moveTime)
        {
            deltaPos *= Time.deltaTime / _moveTime;
            deltaRotRaw *= Time.deltaTime / _moveTime;
        }
        else
        {
            IsMoving = false;
        }
        _elapsedTime += Time.deltaTime;

        var deltaRot = Quaternion.Euler(deltaRotRaw);
        var ev = new ElevatorMoveContentEvent(MainElevator, deltaPos,
            deltaRot, new Bounds(Position, Vector3.one * 10), true);
        Schematic.Position += deltaPos;
        Schematic.Rotation = Quaternion.Euler(currentRot + deltaRotRaw);
        MapEvents.ElevatorMoveContent.RaiseSafely(ev);
    }

    public void MoveToLocation(Vector3 position, Quaternion rotation)
    {
        if (Schematic == null) return;
        IsMoving = true;
        _elapsedTime = 0f;
        _targetPosition = position;
        _targetRotation = rotation;
        _moveTime = (MainElevator as CustomElevator)?.MoveTime ?? 10f;
        /*
         This would be for an Elevator that moves instant
        var deltaPos = position - Position;
        var deltaRot = Quaternion.Euler(rotation.eulerAngles - Rotation.eulerAngles);
        
        var ev = new ElevatorMoveContentEvent(MainElevator, deltaPos,
            deltaRot, new Bounds(Position, Vector3.one * 10), true);
        Schematic.Position = position;
        Schematic.Rotation = rotation;
        MapEvents.ElevatorMoveContent.RaiseSafely(ev);
        */
    }

    public void Generate(CustomElevator elevator,uint schematicId,CustomElevatorDestination destination)
    {
        MainElevator = elevator;
        Schematic = SchematicService.SpawnSchematic(schematicId, destination.Position, destination.Rotation);
    }
}