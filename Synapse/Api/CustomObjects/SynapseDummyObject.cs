using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseDummyObject : DefaultSynapseObject
    {
        public SynapseDummyObject(Vector3 pos, Quaternion rot, Vector3 scale, RoleType role, ItemType held, string name, string badge, string badgecolor)
        {
            Dummy = CreateDummy(pos, rot, scale, role, held, name, badge, badgecolor);

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        internal SynapseDummyObject(SynapseSchematic.DummyConfiguration configuration)
        {
            Dummy = CreateDummy(configuration.Position, configuration.Rotation, configuration.Scale, configuration.Role, configuration.HeldItem, configuration.Name, configuration.Badge, configuration.BadgeColor);
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override GameObject GameObject => Dummy.GameObject;
        public override ObjectType Type => ObjectType.Dummy;
        public override void Destroy() => Dummy.Destroy();
        public override Vector3 Position { get => Dummy.Position; set => Dummy.Position = value; }
        public override Quaternion Rotation { get => Quaternion.Euler(new Vector3(Dummy.Rotation.x, Dummy.Rotation.y, 90f)); set => Dummy.Rotation = new Vector2(value.eulerAngles.x, value.eulerAngles.y); }
        public override Vector3 Scale { get => Dummy.Scale; set => Dummy.Scale = value; }
        public RoleType Role { get => Dummy.Role; set => Dummy.Role = value; }
        public ItemType HeldItem { get => Dummy.HeldItem; set => Dummy.HeldItem = value; }
        public string Name { get => Dummy.Name; set => Dummy.Name = value; }
        public string Badge { get => Dummy.BadgeName; set => Dummy.BadgeName = value; }
        public string BadgeColor { get => Dummy.BadgeColor; set => Dummy.BadgeColor = value; }
        public Dummy Dummy { get; }

        public void Refresh()
        {
            Position = base.Position;
            Rotation = base.Rotation;
            Scale = base.Scale;
        }

        private Dummy CreateDummy(Vector3 pos, Quaternion rot, Vector3 scale, RoleType role, ItemType held, string name, string badge, string badgecolor)
        {
            var dummy = new Dummy(pos, rot, role, name, badge, badgecolor);
            dummy.HeldItem = held;
            dummy.Scale = scale;
            return dummy;
        }
    }
}
