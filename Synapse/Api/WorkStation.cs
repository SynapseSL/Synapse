using System;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Api
{
    public class WorkStation
    {
        internal WorkStation(WorkstationController station) => workStation = station;

        public WorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            //TODO: Check if this still functions
            var bench = UnityEngine.Object.Instantiate(NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
            bench.gameObject.transform.localScale = scale;
            bench.gameObject.transform.position = position;
            bench.gameObject.transform.rotation = Quaternion.Euler(rotation);

            NetworkServer.Spawn(bench);
            workStation = bench.GetComponent<WorkstationController>();

            Map.Get.WorkStations.Add(this);
        }

        public static WorkStation CreateWorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
            => new WorkStation(position, rotation, scale);

        private readonly WorkstationController workStation;

        public GameObject GameObject => workStation.gameObject;

        public string Name => GameObject.name;

        public Vector3 Position => GameObject.transform.position;

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

        public Player KnownUser
        {
            get => workStation._knownUser.GetPlayer();
            set => workStation._knownUser = value.Hub;
        }

        public WorkstationState State
        {
            get => (WorkstationState)workStation.Status;
            set => workStation.NetworkStatus = (byte)value;
        }

        [Obsolete("Tablets are removed from the Game")]
        public bool IsTabletConnected
        {
            get => State == WorkstationState.Online;
            set => State = value ? WorkstationState.Online : WorkstationState.Offline;
        }

        [Obsolete("Tablets are removed from the Game")]
        public SynapseItem ConnectedTablet { get; set; }

        [Obsolete("Tablets are removed from the Game")]
        public Player TabletOwner
        {
            get => KnownUser;
            set => KnownUser = value;
        }
    }
}
