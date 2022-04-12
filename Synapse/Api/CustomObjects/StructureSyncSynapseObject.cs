using MapGeneration.Distributors;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class StructureSyncSynapseObject : NetworkSynapseObject
    {
        private StructurePositionSync Sync { get; set; }

        public override void Refresh()
        {
            //I Still have to find a way to properly Refresh these objects after the first spawn
            //I don't know why but it works?
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
