using MapGeneration.Distributors;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class StructureSyncSynapseObject : NetworkSynapseObject
{
    private StructurePositionSync Sync { get; set; }

    public override GameObject GameObject { get; }

    public override void Refresh()
    {
        Sync.Start();
        base.Refresh();
    }

    protected override TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var obj = base.CreateNetworkObject(component, pos, rot, scale);
        Sync = obj.GetComponent<StructurePositionSync>();
        return obj;
    }
}