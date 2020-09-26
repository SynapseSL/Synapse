using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Generator
    {
        internal Generator(Generator079 gen) => generator = gen;

        private Generator079 generator;

        public GameObject GameObject => generator.gameObject;

        public string Name => GameObject.name;

        public bool Open
        {
            get => generator.NetworkisDoorOpen;
            set
            {
                generator._doorAnimationCooldown = 1.5f;
                generator.NetworkisDoorOpen = value;
                generator.RpcDoSound(!value);
            }
        }

        public bool Locked
        {
            get => !generator.NetworkisDoorUnlocked;
            set
            {
                generator.NetworkisDoorUnlocked = !value;
                generator._doorAnimationCooldown = 0.5f;
            }
        }

        public bool IsTabledConnected
        {
            get => generator.isTabletConnected;
            set
            {
                if (value)
                    generator.isTabletConnected = true;
                else
                    generator.EjectTablet();
            }
        }

        public float RemainingPowerUp
        {
            get => generator.remainingPowerup;
            set => generator.NetworkremainingPowerup = value;
        }

        public Room Room => Map.Get.Rooms.FirstOrDefault(x => x.RoomName.ToLower() == generator.CurRoom.ToLower());

        public Vector3 TabletEjectionPoint => generator.tabletEjectionPoint.position;
    }
}
