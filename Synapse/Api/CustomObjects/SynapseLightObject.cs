using AdminToys;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseLightObject : SynapseToyObject
    {
        public override GameObject GameObject { get; }

        public override ObjectType Type => ObjectType.LightSource;

        public LightSourceToy LightSourceToy { get; }

        public Color LightColor { get; set; }
        public float LightIntensity { get; set; }
        public float LightRange { get; set; }
        public bool LightShadows { get; set; }
    }
}
