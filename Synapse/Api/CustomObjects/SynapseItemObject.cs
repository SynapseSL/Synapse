using Synapse.Api.Enum;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseItemObject : DefaultSynapseObject
    {
        public SynapseItemObject(ItemType type, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Item = CreateItem(type, position, rotation, scale);
            ItemType = type;

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        internal SynapseItemObject(SynapseSchematic.ItemConfiguration configuration)
        {
            Item = CreateItem(configuration.ItemType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
            ItemType = configuration.ItemType;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override GameObject GameObject => Item.PickupBase?.gameObject;
        public override ObjectType Type => ObjectType.Item;

        public override Vector3 Position { get => Item.Position; set => Item.Position = value; }
        public override Quaternion Rotation { get => Item.Rotation; set => Item.Rotation = value; }
        public override Vector3 Scale { get => Item.Scale; set => Item.Scale = value; }
        public override void Destroy() => Item.Destroy();
        public override Rigidbody Rigidbody { get => Item.PickupBase?.Rb; set { } }

        public SynapseItem Item { get; }
        public ItemType ItemType { get; }

        private SynapseItem CreateItem(ItemType type, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var item = new SynapseItem(type);
            item.Rotation = rotation;
            item.Scale = scale;
            item.Position = position;
            item.Drop(position);
            item.PickupBase.Rb.isKinematic = true;
            item.PickupBase.Rb.useGravity = false;
            item.CanBePickedUp = false;
            return item;
        }
    }
}
