using System.Collections.ObjectModel;
using MapGeneration;
using Synapse3.SynapseModule.Map.Objects;

namespace Synapse3.SynapseModule.Map.Rooms;

public interface IVanillaRoom : IRoom
{
    public ReadOnlyCollection<SynapseCamera> Cameras { get; }

    public RoomIdentifier Identifier { get; }

    public FlickerableLightController LightController { get; }

    public RoomType RoomType { get; }
}