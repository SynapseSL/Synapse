using System;
using System.Reflection;
using Synapse.Api;
using UnityEngine;

namespace Synapse.Client
{
    public abstract class SpawnHandler
    {
        public abstract GameObject Spawn(Vector3 pos, Quaternion rot, string name);

        public virtual void AfterSpawnPacket(Player player, GameObject gameObject) { }
        
        public abstract void Destroy(GameObject gameObject);

        public string GetBlueprint()
        {
            if (!(GetType().GetCustomAttribute(typeof(Blueprint)) is Blueprint blueprint))
            {
                throw new Exception("SpawnHandler subclass is not annotated with [Blueprint]");
            }
            return blueprint.Name;
        }
    }
}