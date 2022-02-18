using AdminToys;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseLightObject : SynapseToyObject<LightSourceToy>
    {
        public static LightSourceToy Prefab { get; internal set; }

        public SynapseLightObject(Color color, float lightIntensity, float range, bool shadows, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            ToyBase = CreateLightSource(color, lightIntensity, range, shadows, position, rotation, scale);

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        internal SynapseLightObject(SynapseSchematic.LightSourceConfiguration configuration)
        {
            ToyBase = CreateLightSource(configuration.Color, configuration.LightIntensity, configuration.LightRange, configuration.LightShadows, configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override ObjectType Type => ObjectType.LightSource;
        public override LightSourceToy ToyBase { get; }

        public Color LightColor
        {
            get => ToyBase.LightColor;
            set => ToyBase.NetworkLightColor = value;
        }
        public float LightIntensity
        {
            get => ToyBase.LightIntensity;
            set => ToyBase.NetworkLightIntensity = value;
        }
        public float LightRange
        {
            get => ToyBase.LightRange;
            set => ToyBase.NetworkLightRange = value;
        }
        public bool LightShadows
        {
            get => ToyBase.LightShadows;
            set => ToyBase.NetworkLightShadows = value;
        }

        private LightSourceToy CreateLightSource(Color color, float lightIntensity, float range,bool shadows, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var ot = UnityEngine.Object.Instantiate(Prefab, position, rotation);
            NetworkServer.Spawn(ot.gameObject);

            ot.NetworkLightColor = color;
            ot.NetworkLightIntensity = lightIntensity;
            ot.NetworkLightRange = range;
            ot.NetworkLightShadows = shadows;

            ot.transform.position = position;
            ot.transform.rotation = rotation;
            ot.transform.localScale = scale;
            ot.NetworkScale = scale;

            return ot;
        }
    }
}
