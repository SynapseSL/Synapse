using Elevators;
using Interactables.Interobjects;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Objects;
using UnityEngine;

namespace Synapse3.SynapseModule.Map.Schematic;

public class SynapseObjectScript : MonoBehaviour
{
    private SynapseObjectEvents _events;
    private MapEvents _map;
    private float _nextUpdate = 0f;

    public ISynapseObject Object { get; internal set; }

    public ISynapseObject Parent { get; private set; }

    public bool IsChild { get; private set; }

    public void Start()
    {
        _events ??= Synapse.Get<SynapseObjectEvents>();
        _map ??= Synapse.Get<MapEvents>();

        _events.Load.Raise(new LoadObjectEvent(Object));

        Parent = (Object as DefaultSynapseObject)?.Parent;
        IsChild = Parent != null;

        if (Object is IRefreshable refreshable)
            _nextUpdate = Time.time + refreshable.UpdateFrequency;

        _map.ElevatorMoveContent.Subscribe(MoveElevator);
        if (Object.GameObject.TryGetComponent<ElevatorFollowerBase>(out var follower))
            ElevatorChamber.OnElevatorMoved -= follower.OnElevatorMoved;
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

        _map.ElevatorMoveContent.Subscribe(MoveElevator);
    }


    private bool _inElevator = false;
    private IElevator _elevator = null;
    private Vector3 _lastPosition = default;
    private Transform _transform = null;
    private Transform _previousParent = null;
    private bool _refresh = false;

    private void MoveElevator(ElevatorMoveContentEvent ev)
    {
        if (!Object.MoveInElevator || Object.Parent != null) return;
        var isAlreadyMoving = _inElevator && ev.Elevator == _elevator;
        if (!ev.Bounds.Contains(_lastPosition))
        {
            if (!isAlreadyMoving)
            {
                return;
            }

            _inElevator = false;
            _elevator = null;
            Object.Position -= ev.DeltaPosition;
            Object.GameObject.transform.SetParent(_previousParent);
            if (Object is IRefreshable refreshable)
                refreshable.Update = _refresh;
        }
        else
        {
            if (isAlreadyMoving) return;
            _previousParent = Object.GameObject.transform;
            Object.Position += ev.DeltaPosition;
            _inElevator = true;
            _elevator = ev.Elevator;
            Object.GameObject.transform.SetParent(ev.Elevator.Chamber.ParentTransform);
            if (Object is IRefreshable refreshable)
            {
                _refresh = refreshable.Update;
                refreshable.Update = true;
            }
        }
    }

    private void LateUpdate()
    {
        switch (Object.Type)
        {
            case ObjectType.RagDoll:
                if (Object is not SynapseRagDoll rag) goto default;
                if (rag.Rigidbody.IsSleeping()) return;
                _lastPosition = rag.Rigidbody.position;
                break;

            default:
                _transform ??= transform;
                _lastPosition = _transform.position;
                break;
        }
    }
}