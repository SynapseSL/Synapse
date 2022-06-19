using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public class SynapseObjectScript<TSynapseObject> : MonoBehaviour where TSynapseObject : ISynapseObject
{
    public TSynapseObject Object { get; internal set; }

    //public SynapseObject Parent { get; private set; }

    public bool IsChild { get; private set; }

    public void Start()
    {
        //TODO: Server.Get.Events.SynapseObject.InvokeLoadComponent(new Events.SynapseEventArguments.SOEventArgs(Object));
        
        //Parent = (Object as DefaultSynapseObject)?.Parent;
        //IsChild = Parent != null;
    }

    public void Update()
    {
        //TODO: Server.Get.Events.SynapseObject.InvokeUpdate(new Events.SynapseEventArguments.SOEventArgs(Object));

        if(Object is IRefreshable { UpdateEveryFrame: true } refresh)
            refresh.Refresh();
    }

    public void OnDestroy()
    {
        //TODO: Server.Get.Events.SynapseObject.InvokeDestroy(new Events.SynapseEventArguments.SOEventArgs(Object));
        
        Object.OnDestroy();
        //TODO: Fix this to also remove from the type specific list somehow
        //Parent?.Children.Remove(Object);
    }
}