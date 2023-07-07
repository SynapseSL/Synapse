using Scp914;
using Synapse3.SynapseModule.Item;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Scp914;

public interface ISynapse914Processor
{
    public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default);
}