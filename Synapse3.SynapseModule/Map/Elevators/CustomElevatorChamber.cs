using Ninject;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Elevators;

public class CustomElevatorChamber : ICustomElevatorChamber
{
    private Vector3 _targetPosition;
    private Vector3 _direction;
    private Quaternion _targetRotation;
    private float _moveSpeed = 0f;
    
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
        if (Position == _targetPosition)
        {
            IsMoving = false;
            return;
        }
        
        var deltaPos = _direction * Time.deltaTime * _moveSpeed;
        var currentDifference = _targetPosition - Position;
        var nextDifference = currentDifference - deltaPos;
        
        //The nextDifference normalized is basically the direction * -1 when it would overshoot the destination
        if (nextDifference.normalized != _direction)
        {
            deltaPos = currentDifference;
            IsMoving = false;
        }
        var ev = new ElevatorMoveContentEvent(MainElevator, deltaPos,
            Quaternion.identity, new Bounds(Position, Vector3.one * 10), true);
        Schematic.Position += deltaPos;
        MapEvents.ElevatorMoveContent.RaiseSafely(ev);
    }

    public void MoveToLocation(Vector3 position, Quaternion rotation)
    {
        if (Schematic == null) return;
        IsMoving = true;
        _targetPosition = position;
        _direction = (position - Position).normalized;
        _targetRotation = rotation;
        _moveSpeed = (MainElevator as CustomElevator)?.MoveSpeed ?? 10f;
    }

    public void Generate(CustomElevator elevator,uint schematicId,CustomElevatorDestination destination)
    {
        MainElevator = elevator;
        Schematic = SchematicService.SpawnSchematic(schematicId, destination.Position, destination.Rotation);
    }
}