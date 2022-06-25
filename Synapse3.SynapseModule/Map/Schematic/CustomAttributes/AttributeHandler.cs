using System.Collections.Generic;

namespace Synapse3.SynapseModule.Map.Schematic.CustomAttributes;

public abstract class AttributeHandler
{
    public abstract string Name { get; }

    public List<ISynapseObject> SynapseObjects { get; } = new List<ISynapseObject>();

    public virtual void Init() { }

    public virtual void OnLoad(ISynapseObject synapseObject, System.ArraySegment<string> args) { }

    public virtual void OnDestroy(ISynapseObject synapseObject) { }

    public virtual void OnUpdate(ISynapseObject synapseObject) { }
}