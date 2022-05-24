using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Synapse.Api.Enum;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseWorkStationObject : NetworkSynapseObject
    {
        public static WorkstationController Prefab { get; internal set; }

        //This is just for compatibillity with older Versions
        internal SynapseWorkStationObject(WorkStation station, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            WorkStation = station;
            station.workStation = CreateNetworkObject(Prefab, position, rotation, scale);

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        internal SynapseWorkStationObject(SynapseSchematic.WorkStationConfiguration configuration)
        {
            WorkStation = CreateStation(configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
            CustomAttributes = configuration.CustomAttributes;
            UpdateEveryFrame = configuration.UpdateEveryFrame;

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }
        public SynapseWorkStationObject(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            WorkStation = CreateStation(position, rotation, scale);

            Map.Get.SynapseObjects.Add(this);

            var script = GameObject.AddComponent<SynapseObjectScript>();
            script.Object = this;
        }

        public override GameObject GameObject => WorkStation.GameObject;
        public override NetworkIdentity NetworkIdentity => WorkStation.workStation.netIdentity;
        public override ObjectType Type => ObjectType.Workstation;

        public WorkStation WorkStation { get; }

        private WorkStation CreateStation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var station = new WorkStation(CreateNetworkObject(Prefab, position, rotation, scale));
            Map.Get.WorkStations.Add(station);
            return station;
        }
    }
}
