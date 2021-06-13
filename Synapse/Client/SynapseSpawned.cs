using System;
using Synapse.Client.Packets;
using UnityEngine;

namespace Synapse.Client
{
    public class SynapseSpawned : MonoBehaviour
    {
        public string Blueprint { get; internal set; }
        public bool SyncPosition { get; set; } = false;

        private Transform _transform;
        private Vector3 _lastPos;
        private Quaternion _lastRot;

        public void Awake()
        {
            _transform = transform;
            _lastPos = _transform.position;
            _lastRot = _transform.rotation;
        }

        public void FixedUpdate()
        {
            if (!SyncPosition) return;
            var tp = _transform.position;
            var tr = _transform.rotation;
            if (Vector3.Distance(tp, _lastPos) >= 0.25f || Quaternion.Angle(tr, _lastRot) >= 10)
            {
                SynchronizePosition();
            }
        }

        public void DestroyGlobal()
        {
            SynapseController.ClientManager.SpawnController.Destroy(gameObject, Blueprint);
        }

        public void SynchronizePosition() {   
            _transform = transform;
            _lastPos = _transform.position;
            _lastRot = _transform.rotation;
            ClientPipeline.InvokeBroadcast(PositionPacket.Encode(_transform.position, _transform.rotation, name));
        }

        public static SynapseSpawned ForObject(GameObject gameObject)
        { 
            var ss = gameObject.GetComponent<SynapseSpawned>();
            if (ss == null)
            {
                throw new Exception("GameObject is not spawned via Synapse");
            }

            return ss;
        }
    }
}