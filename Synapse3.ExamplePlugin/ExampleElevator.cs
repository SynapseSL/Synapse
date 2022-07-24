using System.Collections.ObjectModel;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Map.Elevators;

namespace Synapse3.ExamplePlugin;

public class ExampleElevator : CustomElevator
{
    public override ReadOnlyCollection<IElevatorDestination> Destinations { get; }
}