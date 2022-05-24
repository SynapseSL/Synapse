using UnityEngine;

namespace Synapse.Api
{
    public class Camera
    {
        internal Camera(Camera079 camera, Room room)
        {
            _camera = camera;
            Room = room;
        }

        private readonly Camera079 _camera;

        public GameObject GameObject 
            => _camera.gameObject;

        public Room Room { get; private set; }

        public string Name 
            => _camera.cameraName;

        public ushort ID
            => _camera.cameraId;

        public bool IsMainCamera
            => _camera.isMain;
    }
}
