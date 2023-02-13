using System.Collections.ObjectModel;
using MapGeneration;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public interface IVanillaRoom : IRoom
{
    public ReadOnlyCollection<SynapseCamera> Cameras { get; }

    public RoomIdentifier Identifier { get; }

    public FlickerableLightController LightController { get; }

    public Color WarheadColor { get; set; }

    public RoomType RoomType { get; }
}