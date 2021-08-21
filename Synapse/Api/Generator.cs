using System;
using MapGeneration.Distributors;
using Synapse.Api.Items;
using UnityEngine;

namespace Synapse.Api
{
    public class Generator
    {
        internal Generator(Scp079Generator gen)
        {
            generator = gen;
            positionsync = generator.GetComponent<StructurePositionSync>();
        }

        public readonly Scp079Generator generator;

        public readonly StructurePositionSync positionsync;

        public GameObject GameObject => generator.gameObject;

        public string Name => GameObject.name;

        public Vector3 Position
        {
            get => generator.transform.position;
            set => positionsync.Network_position = value;
        }

        public sbyte Rotation
        {
            get => (sbyte)Mathf.RoundToInt(generator.transform.rotation.eulerAngles.y / 5.625f);
            set => positionsync.Network_rotationY = value;
        }

        public bool Open
        {
            get => generator.HasFlag(generator._flags,Scp079Generator.GeneratorFlags.Open);
            set
            {
                generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Open, value);
                generator._targetCooldown = generator._doorToggleCooldownTime;
            }
        }

        public bool Locked
        {
            get => !generator.HasFlag(generator._flags,Scp079Generator.GeneratorFlags.Unlocked);
            set
            {
                generator.ServerSetFlag(Scp079Generator.GeneratorFlags.Unlocked, !value);
                generator._targetCooldown = generator._unlockCooldownTime;
            }
        }

        public bool Active
        {
            get => generator.Activating;
            set
            {
                generator.Activating = value;
                if (value)
                    generator._leverStopwatch.Restart();

                generator._targetCooldown = generator._doorToggleCooldownTime;
            }
        }

        public bool Engaged { get => generator.Engaged; set => generator.Engaged = value; }

        public short Time { get => generator._syncTime; set => generator.Network_syncTime = value; }

        [Obsolete()]
        public Room Room => null;

        [Obsolete("Use Engaged")]
        public bool IsOvercharged => Engaged;

        [Obsolete("Just set Engaged to true")]
        public void Overcharge() => Engaged = true;

        [Obsolete("Use Time instead")]
        public float RemainingPowerUp { get; set; }

        [Obsolete("Use Active instead")]
        public bool IsTabletConnected { get => Active; set => Active = value; }

        [Obsolete("Since 11.0.0 removed")]
        public SynapseItem ConnectedTablet { get; set; }

        [Obsolete("Since 11.0.0 removed")]
        public readonly bool MainGenerator;

        [Obsolete("Since 11.0.0 removed")]
        public Vector3 TabletEjectionPoint => Vector3.zero;
    }
}
