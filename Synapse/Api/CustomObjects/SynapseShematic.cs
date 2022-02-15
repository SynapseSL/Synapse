using System.Collections.Generic;
using UnityEngine;
using System;
using Synapse.Config;

namespace Synapse.Api.CustomObjects
{
    public class SynapseShematic : IConfigSection
    {
        [NonSerialized]
        internal bool reload = true;

        public int ID { get; internal set; }
        public string Name { get; internal set; }

        public List<PrimitiveConfiguration> PrimitiveObjects { get; set; } = new List<PrimitiveConfiguration>();

        public class PrimitiveConfiguration
        {
            public PrimitiveType PrimitiveType { get; set; }

            public SerializedVector3 Position { get; set; }

            public SerializedVector3 Rotation { get; set; }

            public SerializedVector3 Scale { get; set; }

            public SerializedColor Color { get; set; }
        }
    }
}
