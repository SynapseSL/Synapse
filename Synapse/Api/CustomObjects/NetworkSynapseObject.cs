using Mirror;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class NetworkSynapseObject : DefaultSynapseObject
    {
        public abstract NetworkIdentity NetworkIdentity { get; }

        public virtual void Refresh() => NetworkIdentity.UpdatePositionRotationScale();

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

        protected virtual TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale) where TComponent : NetworkBehaviour
        {
            var gameObject = UnityEngine.Object.Instantiate(component, pos, rot);
            gameObject.transform.localScale = scale;
            NetworkServer.Spawn(gameObject.gameObject);
            return gameObject;
        }
    }
}
