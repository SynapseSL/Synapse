using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public class SynapseObjectScript : MonoBehaviour
{
    private SynapseObjectEvents _events;
    private float _nextUpdate = 0f;

    public ISynapseObject Object { get; internal set; }

    public ISynapseObject Parent { get; private set; }

    public bool IsChild { get; private set; }

    public void Start()
    {
        _events = Synapse.Get<SynapseObjectEvents>();
        _events.Load.Raise(new LoadObjectEvent(Object));
        
        Parent = (Object as DefaultSynapseObject)?.Parent;
        IsChild = Parent != null;

        if (Object is IRefreshable refreshable)
            _nextUpdate = Time.time + refreshable.UpdateFrequency;
    }

    public void Update()
    {
        _events.Update.Raise(new UpdateObjectEvent(Object));

        if (Object is IRefreshable { Update: true } refresh)
        {
            if (refresh.UpdateFrequency <= 0)
                refresh.Refresh();

            if (Time.time <= _nextUpdate)
            {
                _nextUpdate = Time.time + refresh.UpdateFrequency;
                refresh.Refresh();
            }
        }
    }

    public void OnDestroy()
    {
        if (Object is not SynapseItem)
            _events.Destroy.Raise(new DestroyObjectEvent(Object));
        Object.OnDestroy();
    }
}