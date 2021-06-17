using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synapse.Client
{
    public class DefaultSpawnHandler : SpawnHandler
    {
        private readonly string _blueprint;
        private readonly Func<GameObject> _producer;

        public DefaultSpawnHandler(string blueprint, Func<GameObject> producer)
        {
            _blueprint = blueprint;
            _producer = producer;
        }

        public override GameObject Spawn(Vector3 pos, Quaternion rot, string name)
        {
            var obj = Object.Instantiate(_producer.Invoke(), pos, rot);
            obj.name = name;
            return obj;
        }

        public override void Destroy(GameObject gameObject) => Object.Destroy(gameObject);

        public override string GetBlueprint() => _blueprint;
    }
}