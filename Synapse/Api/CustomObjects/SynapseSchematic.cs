using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseSchematic
    {
        public int ID { get; internal set; }
        public string Name { get; internal set; }

        public List<PrimitiveConfiguration> PrimitiveObjects { get; set; } = new List<PrimitiveConfiguration>();

        public class PrimitiveConfiguration
        {
            public PrimitiveType PrimitiveType { get; set; }

            public Vector3 Position { get; set; }

            public Vector3 Rotation { get; set; }

            public Vector3 Scale { get; set; }

            public Color Color { get; set; }
        }
    }
}
