using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Schematic;
using UnityEngine;

namespace Synapse3.SynapseModule.Player;

public partial class SynapsePlayer
{
    public void AttachSynapseObject(ISynapseObject so, Vector3 offset)
    {
        so.Rotation = transform.rotation;
        Transform transform1;
        so.Position = (transform1 = transform).TransformPoint(offset);
        so.GameObject.transform.parent = transform1;
    }
    
    public ItemInventory Inventory { get; }
}