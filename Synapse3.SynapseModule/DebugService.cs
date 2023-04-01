using Neuron.Core.Events;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Events;
using System;
using InventorySystem.Items.MicroHID;
using MEC;
using PlayerRoles;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Elevators;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Role;
using UnityEngine;


namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;
    private RoundEvents _round;
    private ItemEvents _item;
    private ScpEvents _scp;
    private ServerEvents _server;
    private EventManager _event;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, ScpEvents scp,
        ServerEvents server, EventManager eventManager)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _server = server;
        _scp = scp;
        _event = eventManager;
    }

    public override void Enable()
    {
        var method = ((Action<IEvent>)Event).Method;
        foreach (var reactor in _event.Reactors)
        {
            if (reactor.Key == typeof(UpdateObjectEvent)) continue;
            if (reactor.Key == typeof(UpdateEvent)) continue;
            if (reactor.Key == typeof(EscapeEvent)) continue;
            if (reactor.Key == typeof(Scp173ObserveEvent)) continue;
            if (reactor.Key == typeof(KeyPressEvent)) continue;
            if (reactor.Key == typeof(SpeakEvent)) continue;
            if (reactor.Key == typeof(SpeakToPlayerEvent)) continue;
            if (reactor.Key == typeof(RoundCheckEndEvent)) continue;
            if (reactor.Key == typeof(SendPlayerDataEvent)) continue;
            if (reactor.Key.IsAbstract) continue;
            reactor.Value.SubscribeUnsafe(this, method);
        }
        _player.KeyPress.Subscribe(OnKeyPress);
        _item.ConsumeItem.Subscribe(ev =>
        {
            if (ev.State == ItemInteractState.Finalize)
                ev.Allow = false;
        });
        _player.Escape.Subscribe(ev =>
        {
            if(ev.EscapeType == EscapeType.NotAssigned)
                Logger.Warn("Escape not assigned");
        });
    }
    
    public void Event(IEvent e)
    {
        switch (e)
        {
            default:
                Logger.Warn("Event triggered: " + e.GetType().Name);
                break;
        }
    }

    private SynapseDummy _dummy;
    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                _dummy = new SynapseDummy(ev.Player.Position, ev.Player.Rotation, RoleTypeId.Scientist, "TestDummy", "Moderator",
                    "red");
                _dummy.RaVisible = true;
                break;
           
            case KeyCode.Alpha2:
                var role = (_dummy.Player.CustomRole as SynapseAbstractRole);
                Logger.Warn("Role Entry " + role.RoleEntry.SeeCondition(ev.Player));
                Logger.Warn("RoleAndUnitEntry " + role.RoleAndUnitEntry.SeeCondition(ev.Player));
                Logger.Warn("LowerRank " + role.PowerStatusEntries[PowerStatus.LowerRank].SeeCondition(ev.Player));
                Logger.Warn("SameRank " + role.PowerStatusEntries[PowerStatus.SameRank].SeeCondition(ev.Player));
                Logger.Warn("HigherRank " + role.PowerStatusEntries[PowerStatus.HigherRank].SeeCondition(ev.Player));
                break;
            case KeyCode.Alpha3:
                Logger.Warn((ev.Player.Room as IVanillaRoom).Identifier.gameObject.name);
                break;
            
            case KeyCode.Alpha4:
                if (!Physics.Raycast(ev.Player.Position, Vector3.down, out var raycastHit, 3f, MicroHIDItem.WallMask))
                {
                    return;
                }

                Synapse.Get<MapService>().SpawnTantrum(ev.Player.Position + Vector3.up * 0.25f);
                break;
        }
    }
}
#endif
