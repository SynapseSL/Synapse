namespace Synapse3.SynapseModule.Map.Elevators;

public interface ICustomElevator : IElevator
{
    public ElevatorAttribute Attribute { get; set; }

    public void Load();
    public void Generate();
    public void Unload();
}