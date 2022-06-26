using System;
using Mirror;
using Neuron.Core.Logging;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse3.SynapseModule.Item;

public partial class SynapseItem
{
    public void PickUp(SynapsePlayer player)
    {
        
    }

    public void Drop(Vector3 position)
    {
        
    }

    public void Drop()
    {
        
    }

    public void Despawn()
    {
        
    }

    public override void Destroy()
    {
        Despawn();
        
        base.Destroy();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    
    private void DespawnItemBase()
    {
        if(Item is null) return;

        var holder = ItemOwner;
        if (holder != null)
        {
            Item.OnRemoved(null);

            //TODO: Set Current ItemInHand to None
            
            holder.VanillaInventory.UserInventory.Items.Remove(Serial);
            holder.VanillaInventory.SendItemsNextFrame = true;
        }
        
        Object.Destroy(Item.gameObject);
    }

    private void DespawnPickup()
    {
        if(Pickup == null) return;

        NetworkServer.Destroy(Pickup.gameObject);
    }

    internal void UpdateSchematic()
    {
        try
        {
            if(Schematic is null || Pickup is null) return;

            Schematic = new SynapseSchematic(SchematicConfiguration);
            Schematic.Position = Position;
            Schematic.Rotation = Rotation;
            Schematic.Scale = Scale;
            Schematic.Parent = this;
            
            Pickup.netIdentity.DespawnForAllPlayers();
        }
        catch (Exception ex)
        {
            NeuronLogger.For<Synapse>()
                .Error($"Sy3 Item: Creating schematic {SchematicConfiguration?.ID} failed for item {Name}\n" + ex);
        }
    }
}