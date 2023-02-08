using Scp914;
using Scp914.Processors;
using Synapse3.SynapseModule.Item;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Scp914;

public class Default914Processor : ISynapse914Processor
{
    
    private readonly Scp914ItemProcessor _defaultProcessor;

    public Default914Processor(Scp914ItemProcessor processor)
    {
        _defaultProcessor = processor;
    }

    public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
    {
        if(_defaultProcessor == null) return false;
        
        switch (item.State)
        {
            case ItemState.Inventory:
                _defaultProcessor.OnInventoryItemUpgraded(setting, item.ItemOwner, item.Serial);
                return true;
            
            case ItemState.Map:
                _defaultProcessor.OnPickupUpgraded(setting, item.Pickup,
                    position == default ? item.Position : position);
                return true;
        }
        return false;
    }
}