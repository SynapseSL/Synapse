using Synapse.Api.Enum;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseItemObject : DefaultSynapseObject
    {
        public SynapseItemObject(ItemType type, Vector3 position, Quaternion rotation, Vector3 scale, bool pickup = false)
        {
            Item = CreateItem(type, position, rotation, scale, pickup);
            ItemType = type;

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        internal SynapseItemObject(SynapseSchematic.ItemConfiguration configuration)
        {
            Item = CreateItem(configuration.ItemType, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale, configuration.CanBePickedUp);
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
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
        public override void ApplyPhysics()
        {
            Item.PickupBase.Rb.isKinematic = false;
            Item.PickupBase.Rb.useGravity = true;
        }

        public SynapseItem Item { get; }
        public bool CanBePickedUp { get => Item.CanBePickedUp; set => Item.CanBePickedUp = value; }
        public ItemType ItemType { get; }

        private SynapseItem CreateItem(ItemType type, Vector3 position, Quaternion rotation, Vector3 scale, bool pickup = false)
        {
            var item = new SynapseItem(type);
            item.Rotation = rotation;
            item.Scale = scale;
            item.Position = position;
            item.Drop(position);
            item.PickupBase.Rb.isKinematic = true;
            item.PickupBase.Rb.useGravity = false;
            item.CanBePickedUp = pickup;
            return item;
        }
    }
}
