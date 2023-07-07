using Neuron.Core.Meta;
using Scp914;
using Synapse3.SynapseModule.Events;
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
    BasedItemType = ItemType.GunCOM15
)]
public class ExampleCustomItem : CustomItemHandler, ISynapse914Processor
{
    public ExampleCustomItem(ItemEvents items, PlayerEvents player) : base(items, player) { }
    
    //Destroy the custom item 99 and create a coin
    public bool CreateUpgradedItem(SynapseItem item, Scp914KnobSetting setting, Vector3 position = default)
    {
        var state = item.State;
        var owner = item.ItemOwner;
        item.Destroy();

        switch (state)
        {
            case ItemState.Map:
                new SynapseItem(ItemType.Coin, position);
                return true;
                
            case ItemState.Inventory:
                new SynapseItem(ItemType.Coin, owner);
                return true;
        }
        return false;
    }

    public override void OnEquip(ChangeItemEvent ev)
    {
        ev.Player.SendHint("You Equipped my Custom Weapon!");
    }

    //This means you can load up to 6 Adrenaline inside your Weapon at the same time and gain up to 3 Shoots (3 Shoots that are each "shooting" 2 adrenaline)
    public override bool VanillaReload => false;
    public override uint AmmoType => (uint)ItemType.Adrenaline;
    public override int MagazineSize => 3;
    public override bool Reloadable => true;
    public override int NeededForOneShot => 2;
}