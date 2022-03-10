using MapGeneration.Distributors;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class StructureSyncSynapseObject : NetworkSynapseObject
    {
        private StructurePositionSync Sync { get; set; }

        public override void Refresh()
        {
            //I Still have to finde a way to properly Refresh these objects after the first spawn
            Sync.Start();
            base.Refresh();
        }

        protected override TComponent CreateNetworkObject<TComponent>(TComponent component, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            var obj = base.CreateNetworkObject(component, pos, rot, scale);
            Sync = obj.GetComponent<StructurePositionSync>();
            return obj;
        }
    }
}
