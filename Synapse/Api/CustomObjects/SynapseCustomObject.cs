using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    /// <summary>
    /// This object is not really usefull for a plugin directly. It is more intended to be used in a Editor to mark a special loctaion that a plugin could use. Example: Role Spawn location
    /// </summary>
    public class SynapseCustomObject : DefaultSynapseObject
    {
        public SynapseCustomObject(Vector3 position, Vector3 rotation, Vector3 scale, int id)
        {
            GameObject = new GameObject("SynapseCustomObject-" + id);
            ID = id;
            Position = position;
            Rotation = Quaternion.Euler(rotation);
            Scale = scale;

            Map.Get.SynapseObjects.Add(this);
            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        internal SynapseCustomObject(SynapseSchematic.CustomObjectConfiguration configuration)
        {
            GameObject = new GameObject("SynapseCustomObject-" + configuration.ID);
            GameObject.transform.position = configuration.Position;
            GameObject.transform.rotation = configuration.Rotation;
            GameObject.transform.localScale = configuration.Scale;
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
            ID = configuration.ID;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public int ID { get; }

        public override GameObject GameObject { get; }
        public override ObjectType Type => ObjectType.Custom;
        public override void Destroy()
            => Object.Destroy(GameObject);
    }
}
