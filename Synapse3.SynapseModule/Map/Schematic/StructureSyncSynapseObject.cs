using MapGeneration.Distributors;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class StructureSyncSynapseObject : NetworkSynapseObject
{
    private StructurePositionSync Sync { get; set; }

    public override Vector3 Position
    {
        set
        {
            Sync._position = value;
            Sync.Network_position = value;
            base.Position = value;
        }
    }

    public override Quaternion Rotation
    {
        set
        {
            var rot = (sbyte)Mathf.RoundToInt(value.y / 5.625f);
            Sync._rotationY = rot;
            Sync.Network_rotationY = rot;
            base.Rotation = value;
        }
    }

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