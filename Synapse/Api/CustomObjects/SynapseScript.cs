using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseScript : MonoBehaviour
    {
        internal ISynapseObject SynapseObject { get; set; }

        private void Update()
            => Server.Get.Events.SynapseObject.InvokeUpdate(new Events.SynapseEventArguments.SOEventArgs(SynapseObject));
    }
}
