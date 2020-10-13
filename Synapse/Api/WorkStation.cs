using Mirror;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Api
{
    public class WorkStation
    {
        internal WorkStation(global::WorkStation station)
        {
            workStation = station;
        }

        public WorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            var bench = Object.Instantiate(NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
            bench.gameObject.transform.localScale = scale;

            NetworkServer.Spawn(bench);
            var offset = new Offset();
            offset.position = position;
            offset.rotation = rotation;
            offset.scale = Vector3.one;
            workStation = bench.GetComponent<global::WorkStation>();
            workStation.Networkposition = offset;
            bench.AddComponent<WorkStationUpgrader>();

            Map.Get.WorkStations.Add(this);
        }

        private readonly global::WorkStation workStation;

        public GameObject GameObject => workStation.gameObject;

        public string Name => GameObject.name;

        public Vector3 Position => workStation.Networkposition.position;

        public Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                NetworkServer.UnSpawn(GameObject);
                GameObject.transform.localScale = value;
                NetworkServer.Spawn(GameObject);
            }
        }

        public bool IsTabletConnected
        {
            get => workStation.NetworkisTabletConnected;
            set
            {

                if (value)
                {
                    workStation.NetworkisTabletConnected = true;
                    workStation._animationCooldown = 6.5f;
                }
                else
                {
                    if (ConnectedTablet != null && TabletOwner != null)
                        ConnectedTablet.PickUp(TabletOwner);
                    TabletOwner = null;
                    workStation.NetworkisTabletConnected = false;
                    workStation._animationCooldown = 3.5f;
                    
                }
            }
        }

        private SynapseItem connectedtablet;
        public SynapseItem ConnectedTablet
        {
            get => connectedtablet;
            set
            {
                connectedtablet = value;

                if (value != null)
                {
                    IsTabletConnected = true;
                    value.Despawn();
                }
                else
                    IsTabletConnected = false;
            }
        }

        public Player TabletOwner
        {
            get => workStation.Network_playerConnected == null ? null : workStation.Network_playerConnected.GetPlayer();
            set
            {
                if (value == null)
                    workStation.Network_playerConnected = null;
                else
                    workStation.Network_playerConnected = value.gameObject;
            }
        }
    }
}
