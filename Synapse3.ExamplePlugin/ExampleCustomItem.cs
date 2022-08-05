using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Scp914;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Scp914;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

[Automatic]
//You don't have to also create a Processor in the same class but that way others can see how this CustomItem is upgraded more easily
[Scp914Processor(ReplaceHandlers = new uint[]
{
    99
})]
[Item(
    Name = "Example",
    Id = 99,
    BasedItemType = ItemType.Coin,
    SchematicID = 1
)]
public class ExampleCustomItem : CustomItemHandler, ISynapse914Processor
{
    public ExampleCustomItem()
    {
        NeuronLogger.For<ExamplePlugin>().Warn("CREATED ITEM HANDLER/914 PROCESSOR");
    }
    
    //Destroy the custom item 99 and create a "real" coin
    public void CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
    {
        var state = item.State;
        var owner = item.ItemOwner;
        item.Destroy();

        switch (state)
        {
            case ItemState.Map:
                new SynapseItem(ItemType.Coin, position);
                break;
                
            case ItemState.Inventory:
                new SynapseItem(ItemType.Coin, owner);
                break;
        }
    }
}