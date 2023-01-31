using MapGeneration.Distributors;
using Mirror;
using UnityEngine;
using Utils.Networking;

namespace Synapse3.SynapseModule.Map.Schematic;

public abstract class StructureSyncSynapseObject : NetworkSynapseObject
{
    private StructurePositionSync Sync { get; set; }

    public override GameObject GameObject { get; }

    public override void Refresh()
    {
        //TODO:
        Sync.Network_position = Position;
        Sync.Network_rotationY = (sbyte)Mathf.RoundToInt(Rotation.eulerAngles.y / 5.625f);
        NetworkServer.UnSpawn(GameObject);
        NetworkServer.Spawn(GameObject);
    }

    protected override TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var obj = base.CreateNetworkObject(component, pos, rot, scale);
        Sync = obj.GetComponent<StructurePositionSync>();
        return obj;
    }
}