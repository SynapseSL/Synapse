using UnityEngine;

namespace Synapse.Api
{
    public class SynapseCamera
    {
        internal SynapseCamera(Camera079 camera,Room room)
        {
            cam = camera;
            Room = room;
            Map.Get.Cameras.Add(this);
        }

        private readonly Camera079 cam;

        public GameObject GameObject => cam.gameObject;

        public Room Room { get; private set; }

        public string Name => cam.cameraName;

        public ushort ID => cam.cameraId;

        public bool IsMainCamera => cam.isMain;
    }
}
