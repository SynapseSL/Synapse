using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseObjectScript : MonoBehaviour
    {
        public ISynapseObject Object { get; internal set; }

        public void Start()
            => Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(Object));

        public void Update()
            => Server.Get.Events.SynapseObject.InvokeUpdate(new Events.SynapseEventArguments.SOEventArgs(Object));
    }
}
