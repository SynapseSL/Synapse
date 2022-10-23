using System.Collections.Generic;
using System.Collections.ObjectModel;
using MEC;
using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Map.Elevators;

public class SynapseElevator : DefaultElevator
{
    public Lift Lift { get; }
    
    public SynapseElevator(Lift lift)
    {
        Lift = lift;
        var list = new List<IElevatorDestination>();
        for (uint i = 0; i < lift.elevators.Length; i++)
        {
            list.Add(new VanillaDestination(this, lift.elevators[i], lift.elevatorName + "-" + i, i));
        }

        Destinations = list.AsReadOnly();
        CurrentDestination = list[0];
    }
    
    public override string Name => Lift.elevatorName;
    public override uint Id => (uint)ElevatorType;
    
    public override bool Locked 
    {
        get => Lift._locked;
        set => Lift.SetLock(Locked, value);
    }

    public override bool IsMoving => Lift.status == Lift.Status.Moving;

    public override ReadOnlyCollection<IElevatorDestination> Destinations { get; }
    
    public Lift.Status Status
    {
        get => Lift.status;
        set => Lift.SetStatus((byte)value);
    }
    
    public bool Operative => Lift.operative;

    public override void MoveToDestination(uint destinationId)
    {
        if (destinationId is 0 or 1 && destinationId != CurrentDestination.ElevatorId)
            Timing.RunCoroutine(_MoveTo(destinationId));
    }

    private IEnumerator<float> _MoveTo(uint destinationId)
    {
        var previous = Lift.status;
        Lift.SetStatus(2);

        for (int i = 0; i < 35; i++)
        {
            yield return Timing.WaitForOneFrame;
        }
        
        Lift.RpcPlayMusic();

        for (int i = 0; i < 100; i++)
        {
            yield return Timing.WaitForOneFrame;
        }
        
        MoveContent(destinationId);

        for (int i = 0; i < (Lift.movingSpeed - 2f) * 50f; i++)
        {
            yield return Timing.WaitForOneFrame;
        }
        
        Lift.SetStatus((byte)(previous == Lift.Status.Down ? 0 : 1));

        for (int i = 0; i < 150; i++)
        {
            yield return Timing.WaitForOneFrame;
        }

        Lift.operative = true;
        CurrentDestination = GetDestination(destinationId);
    }


    /// <summary>
    /// The type of the elevator
    /// </summary>
    public ElevatorType ElevatorType
    {
        get
        {
            return Name switch
            {
                "GateB" => ElevatorType.GateB,
                "GateA" => ElevatorType.GateA,
                "SCP-049" => ElevatorType.Scp049,
                "ElA" => ElevatorType.ElALeft,
                "ElA2" => ElevatorType.ElARight,
                "ElB" => ElevatorType.ElBLeft,
                "ElB2" => ElevatorType.ElBRight,
                _ => ElevatorType.None,
            };
        }
    }

    public override string ToString() => Name;
}