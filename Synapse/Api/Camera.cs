using UnityEngine;

namespace Synapse.Api
{
    public class Camera
    {
        internal Camera(Camera079 camera, Room room)
        {
            cam = camera;
            Room = room;
        }

        private readonly Camera079 cam;

        public GameObject GameObject => cam.gameObject;

        public Room Room { get; private set; }

        public string Name => cam.cameraName;

        public ushort ID => cam.cameraId;

        public bool IsMainCamera => cam.isMain;
    }
}
