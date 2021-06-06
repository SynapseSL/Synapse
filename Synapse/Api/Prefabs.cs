using UnityEngine;
using Interactables.Interobjects.DoorUtils;

namespace Synapse.Api
{
    public class Prefabs
    {
        public static Prefabs Get => Server.Get.Prefabs;

        internal Prefabs() { }

        public DoorVariant DoorVariantPrefab { get; internal set; }
    }
}
