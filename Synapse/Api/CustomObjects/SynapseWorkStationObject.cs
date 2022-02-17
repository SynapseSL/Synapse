using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using Synapse.Api.Enum;
using System;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseWorkStationObject : DefaultSynapseObject
    {
        public static WorkstationController Prefab { get; set; }

        //This is just for compatibillity with older Versions
        internal SynapseWorkStationObject(WorkStation station, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            WorkStation = station;
            station.workStation = CreateController(position, rotation, scale);

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }
        internal SynapseWorkStationObject(SynapseSchematic.WorkStationConfiguration configuration)
        {
            WorkStation = CreateStation(configuration.Position, Quaternion.Euler(configuration.Rotation), configuration.Scale);
            OriginalScale = configuration.Scale;
        }
        public SynapseWorkStationObject(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            WorkStation = CreateStation(position, rotation, scale);

            Map.Get.SynapseObjects.Add(this);
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(this));
        }


        public override GameObject GameObject => WorkStation.GameObject;
        public override ObjectType Type => ObjectType.Workstation;
        public override Vector3 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                Refresh();
            }
        }
        public override Quaternion Rotation
        {
            get => base.Rotation;
            set
            {
                base.Rotation = value;
                Refresh();
            }
        }
        public override Vector3 Scale
        {
            get => base.Scale;
            set
            {
                base.Scale = value;
                Refresh();
            }
        }


        public WorkStation WorkStation { get; }


        public void Refresh()
            => WorkStation.workStation.netIdentity.UpdatePositionRotationScale();


        private WorkStation CreateStation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var station = new WorkStation(CreateController(position, rotation, scale));
            Map.Get.WorkStations.Add(station);
            return station;
        }
        private WorkstationController CreateController(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var ot = UnityEngine.Object.Instantiate(Prefab, position, rotation);
            ot.transform.position = position;
            ot.transform.rotation = rotation;
            ot.transform.localScale = scale;
            NetworkServer.Spawn(ot.gameObject);
            return ot;
        }
    }
}
