namespace Synapse3.SynapseModule.Item.SubAPI;

public interface ISubSynapseItem
{
    public float Durability { get; set; }

    public void ChangeState(ItemState newState);
}