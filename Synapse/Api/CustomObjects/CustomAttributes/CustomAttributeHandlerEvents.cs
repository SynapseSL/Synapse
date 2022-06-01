using Synapse.Api.Events.SynapseEventArguments;
using System;
using static Synapse.Api.Events.EventHandler;

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
        public void OnLoad(SOEventArgs ev)
        {
            foreach (var handler in Handlers)
            {
                var name = handler.Name;

                foreach (var attribute in ev.Object.CustomAttributes)
                {
                    if (attribute is null)
                        continue;

                    var args = attribute.Split(':');
                    if (!args[0].Equals(handler.Name, StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    var newargs = args.Segment(1);

                    handler.SynapseObjects.Add(ev.Object);
                    handler.OnLoad(ev.Object, newargs);
                    return;
                }
            }
        }
        public void OnDestroy(SOEventArgs ev)
        {
            foreach (var handler in Handlers)
            {
                if (handler.SynapseObjects.Contains(ev.Object))
                {
                    handler.OnDestroy(ev.Object);
                    _ = handler.SynapseObjects.Remove(ev.Object);
                }
            }
        }
        public void OnUpdate(SOEventArgs ev)
        {
            foreach (var handler in Handlers)
            {
                if (handler.SynapseObjects.Contains(ev.Object))
                    handler.OnUpdate(ev.Object);
            }
        }
    }
}
