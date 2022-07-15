﻿using Neuron.Core.Meta;
using Scp914;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Scp914;
using UnityEngine;

namespace Synapse3.ExamplePlugin;

[Automatic]
[Scp914Processor(ReplaceHandlers = new []
{
    0,1,2,3,4,5,6,7,8,9,10,11 // This means that this Processor wil be responsible for all Items with the ID 0-11 (all Keycards)
})]
public class ExampleScp914Processor : ISynapse914Processor
{
    public void CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
    {
        var type = setting switch
        {
            Scp914KnobSetting.Coarse => ItemType.KeycardJanitor,
            Scp914KnobSetting.Rough => ItemType.KeycardScientist,
            Scp914KnobSetting.OneToOne => ItemType.KeycardResearchCoordinator,
            Scp914KnobSetting.Fine => ItemType.KeycardNTFCommander,
            Scp914KnobSetting.VeryFine => ItemType.KeycardO5,
            _ => ItemType.None
        };
        
        if(type == ItemType.None) return;
        var state = item.State;
        var owner = item.ItemOwner;
        
        //The Plugin is responsible for destroying the old item
        item.Destroy();

        switch (state)
        {
            case ItemState.Map:
                new SynapseItem(type, position);
                break;
                
            case ItemState.Inventory:
                new SynapseItem(type, owner);
                break;
        }
    }
}