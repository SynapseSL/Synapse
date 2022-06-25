using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public class SynapseObjectScript : MonoBehaviour
{
    private SynapseObjectEvents _events;

    public ISynapseObject Object { get; internal set; }

    public ISynapseObject Parent { get; private set; }

    public bool IsChild { get; private set; }

    public void Start()
    {
        _events = Synapse.Get<SynapseObjectEvents>();
        _events.LoadObject.Raise(new LoadObjectEvent(Object));
        
        Parent = (Object as DefaultSynapseObject)?.Parent;
        IsChild = Parent != null;
    }

    public void Update()
    {
        _events.UpdateObject.Raise(new UpdateObjectEvent(Object));

        if(Object is IRefreshable { UpdateEveryFrame: true } refresh)
            refresh.Refresh();
    }

    public void OnDestroy()
    {
        _events.DestroyObject.Raise(new DestroyObjectEvent(Object));
        Object.OnDestroy();
    }
}