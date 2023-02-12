using System.Collections.ObjectModel;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Rooms;

public interface IVanillaRoom : IRoom
{
    public ReadOnlyCollection<SynapseCamera> Cameras { get; }

    public Color WarheadColor { get; set; }
}