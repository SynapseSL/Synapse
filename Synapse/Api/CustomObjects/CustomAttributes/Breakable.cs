using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public class Breakable : AttributeHandler
    {
        public override string Name => "Breakable";

        public override void OnLoad(ISynapseObject synapseObject)
        {
            Logger.Get.Debug("Loaded: " + synapseObject.GameObject.name);
        }

        public override void OnDestroy(ISynapseObject synapseObject)
        {
            Logger.Get.Debug("Destroy: " + synapseObject.GameObject.name);
        }
    }
}
