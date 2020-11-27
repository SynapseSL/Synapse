using System.Linq;
using UnityEngine;

namespace Synapse.Api
{
    public class Generator
    {
        internal Generator(Generator079 gen,bool main)
        {
            generator = gen;
            MainGenerator = main;
        }

        private Generator079 generator;

        public GameObject GameObject => generator.gameObject;

        public readonly bool MainGenerator;

        public string Name => GameObject.name;

        public Vector3 Position => GameObject.transform.position;

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
                if (value == Locked) return;
                generator.NetworkisDoorUnlocked = !value;
                generator._doorAnimationCooldown = 0.5f;
            }
        }

        public bool IsTabletConnected
        {
            get => generator.isTabletConnected;
            set
            {
                if (value)
                {
                    if (!IsTabletConnected)
                        generator.NetworkisTabletConnected = true;
                }
                else
                {
                    if(IsTabletConnected)
                        generator.EjectTablet();
                }
            }
        }

        private Items.SynapseItem tablet;
        public Items.SynapseItem ConnectedTablet
        {
            get => tablet;
            set
            {
                tablet = value;

                if (value != null)
                {
                    IsTabletConnected = true;
                    value.Despawn();
                }
                else
                    IsTabletConnected = false;
            }
        }

        public float RemainingPowerUp
        {
            get => generator.remainingPowerup;
            set => generator.SetTime(value);
        }

        public Room Room => Map.Get.Rooms.FirstOrDefault(x => x.RoomName.ToLower() == generator.CurRoom.ToLower());

        public Vector3 TabletEjectionPoint => generator.tabletEjectionPoint.position;

        public bool IsOvercharged => Map.Get.HeavyController.ForcedOvercharge || generator._localTime <= 0f;

        public void Overcharge()
        {
            if (IsOvercharged) return;
            Map.Get.HeavyController.ActiveGenerators++;
            generator.NetworkremainingPowerup = 0f;
            generator._localTime = 0f;
            generator.EjectTablet();
            generator.RpcNotify(Map.Get.HeavyController.ActiveGenerators);
            if (Map.Get.HeavyController.ActiveGenerators < 5)
                Respawning.RespawnEffectsController.PlayCassieAnnouncement(string.Concat(new object[]
                {
                        "JAM_",
                        UnityEngine.Random.Range(0, 100).ToString("000"),
                        "_",
                        UnityEngine.Random.Range(2, 5),
                        " SCP079RECON",
                        Map.Get.HeavyController.ActiveGenerators
                }), false, true);
            else
                Recontainer079.BeginContainment(false);
        }
    }
}
