using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public class SynapseObjectScript : MonoBehaviour
    {
        public ISynapseObject Object { get; internal set; }

        public SynapseObject Parent { get; private set; }

        public bool IsChild { get; private set; }

        public void Start()
        {
            Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(Object));
            Parent = (Object as DefaultSynapseObject).Parent;
            IsChild = Parent != null;
        }

        public void Update()
        {
            Server.Get.Events.SynapseObject.InvokeUpdate(new Events.SynapseEventArguments.SOEventArgs(Object));

            if (Object is IRefreshable refresh && refresh.UpdateEveryFrame)
                refresh.Refresh();
        }

        public void OnDestroy()
        {
            Server.Get.Events.SynapseObject.InvokeDestroy(new Events.SynapseEventArguments.SOEventArgs(Object));

            _ = (Parent?.Childrens.Remove(Object));

            if (Map.Get.SynapseObjects.Contains(Object))
                _ = Map.Get.SynapseObjects.Remove(Object);
        }
    }
}
