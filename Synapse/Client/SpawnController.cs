using System;
using System.Collections.Generic;
using Synapse.Api;
using Synapse.Client.Packets;
using UnityEngine;
using Logger = Synapse.Api.Logger;

namespace Synapse.Client
{
    public class SpawnController
    {
        //Maybe replace with just a string
        public List<GameObject> SpawnedObjects { get; internal set; } = new List<GameObject>();

        private Dictionary<string, SpawnHandler> Blueprints { get; set; } =
            new Dictionary<string, SpawnHandler>();

        public void Register(SpawnHandler handler)
        {
            Blueprints[handler.GetBlueprint()] = handler;
        }

        public void SpawnLate(Player player)
        {
            try
            {
                foreach (var gameObject in SpawnedObjects)
                {
                    var ss = SynapseSpawned.ForObject(gameObject);
                    SpawnFor(player, gameObject, ss.Blueprint);
                }
            }
            catch (Exception e)
            {
                Logger.Get.Error(e);
            }
        }

        public SynapseSpawned Spawn(Vector3 pos, Quaternion rot, string name, string blueprint)
        {
            var handler = Blueprints[blueprint];
            var gameObject = handler.Spawn(pos, rot, name);
            var ss = gameObject.AddComponent<SynapseSpawned>();
            ss.Blueprint = blueprint;
            SpawnedObjects.Add(gameObject);

            foreach (var serverPlayer in SynapseController.Server.Players)
            {
                SpawnFor(serverPlayer, gameObject, blueprint);
            }

            return ss;
        }

        public void Destroy(GameObject gameObject, string blueprint)
        {
            SpawnedObjects.Remove(gameObject);
            var handler = Blueprints[blueprint];
            foreach (var serverPlayer in SynapseController.Server.Players)
            {
                DestroyFor(serverPlayer, gameObject, blueprint);
            }
            handler.Destroy(gameObject);
        }
        
        public void SpawnFor(Player player, GameObject gameObject, string blueprint)
        {
            var handler = Blueprints[blueprint];
            var transform = gameObject.transform; 
            ClientPipeline.Invoke(player, SpawnPacket.Encode(transform.position, transform.rotation, gameObject.name, blueprint));
            handler.AfterSpawnPacket(player, gameObject);
        }
        
        public void DestroyFor(Player player, GameObject gameObject, string blueprint)
        {
            //var handler = Blueprints[blueprint];
            ClientPipeline.Invoke(player, DestroyPacket.Encode(gameObject.name, blueprint));
        }
    }

    public class Blueprint : Attribute
    {
        public string Name { get; set; }
    }
}