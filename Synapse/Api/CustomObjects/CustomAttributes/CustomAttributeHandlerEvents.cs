using static Synapse.Api.Events.EventHandler;
using System.Linq;

namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public partial class CustomAttributeHandler
    {
        public void RegisterEvents()
        {
            Get.SynapseObject.LoadComponentEvent += OnLoad;
            Get.SynapseObject.DestroyEvent += OnDestroy;
            Get.SynapseObject.UpdateEvent += OnUpdate;
        }

        public void OnLoad(Events.SynapseEventArguments.SOEventArgs ev)
        {
            foreach (var handler in Handlers)
            {
                if (!ev.Object.CustomAttributes.Any(x => x.ToLower().Contains(handler.Name.ToLower()))) continue;

                handler.SynapseObjects.Add(ev.Object);
                handler.OnLoad(ev.Object);
            }
        }

        public void OnDestroy(Events.SynapseEventArguments.SOEventArgs ev)
        {
            foreach (var handler in Handlers)
            {
                if (handler.SynapseObjects.Contains(ev.Object))
                {
                    handler.OnDestroy(ev.Object);
                    handler.SynapseObjects.Remove(ev.Object);
                }
            }
        }

        public void OnUpdate(Events.SynapseEventArguments.SOEventArgs ev)
        {
            foreach (var handler in Handlers)
            {
                if (handler.SynapseObjects.Contains(ev.Object))
                    handler.OnUpdate(ev.Object);
            }
        }
    }
}
