using System;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using UnityEngine;
using Synapse.Api.CustomObjects;

namespace Synapse.Api
{
    public class WorkStation
    {
        internal WorkStation(WorkstationController station) => workStation = station;

        [Obsolete("Please create a Synapse.Api.CustomObjects.SynapseWorkStationObject")]
        public WorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
            => new SynapseWorkStationObject(position, Quaternion.Euler(rotation), scale);

        [Obsolete("Please create a Synapse.Api.CustomObjects.SynapseWorkStationObject")]
        public static WorkStation CreateWorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
            => new WorkStation(position, rotation, scale);

        internal WorkstationController workStation;

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
