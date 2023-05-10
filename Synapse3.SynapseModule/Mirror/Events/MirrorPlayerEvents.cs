using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using Neuron.Core.Meta;
using PlayerRoles;
using PluginAPI.Enums;
using PluginAPI.Events;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Elevators;

namespace Synapse3.SynapseModule.Mirror.Events;

public class MirrorPlayerEvents : Service
{
    private readonly PlayerEvents _playerEvents;
    private readonly RoundEvents _roundEvents;

    public MirrorPlayerEvents(PlayerEvents playerEvents,RoundEvents roundEvents)
    {
        _playerEvents = playerEvents;
        _roundEvents = roundEvents;
    }
    
    public override void Enable()
    {
        _roundEvents.Waiting.Subscribe(Waiting);
    }

    public override void Disable()
    {
        _roundEvents.Waiting.Unsubscribe(Waiting);
    }

    private void Waiting(RoundWaitingEvent ev)
    {
        NetworkServer.ReplaceHandler<ElevatorManager.ElevatorSyncMsg>(ElevatorMessage);
    }

    private void ElevatorMessage(NetworkConnection connection, ElevatorManager.ElevatorSyncMsg msg)
    {
        ReferenceHub referenceHub;
        if (!ReferenceHub.TryGetHubNetID(connection.identity.netId, out referenceHub))
        {
            return;
        }
        if (!referenceHub.IsAlive())
        {
            return;
        }
        msg.Unpack(out var elevatorGroup, out var lvl);
        if (!ElevatorManager.SpawnedChambers.TryGetValue(elevatorGroup, out var elevatorChamber) || elevatorChamber == null)
        {
            return;
        }
        if (!elevatorChamber.IsReady)
        {
            return;
        }
        foreach (ElevatorPanel elevatorPanel in elevatorChamber.AllPanels)
        {
            if (elevatorPanel.AssignedChamber.AssignedGroup != elevatorGroup ||
                (elevatorPanel.AssignedChamber.ActiveLocks != DoorLockReason.None &&
                 !referenceHub.serverRoles.BypassMode) ||
                !elevatorPanel.VerificationRule.ServerCanInteract(referenceHub, elevatorPanel)) continue;

            var elevator = elevatorChamber.GetSynapseElevator() as SynapseElevator;
            if(elevator == null) break;
            var ev = new CallVanillaElevatorEvent(referenceHub.GetSynapsePlayer(), EventManager.ExecuteEvent(
                ServerEventType.PlayerInteractElevator, new object[]
                {
                    referenceHub,
                    elevatorChamber
                }), elevator, elevator.Destinations[lvl] as SynapseElevatorDestination);
            _playerEvents.CallVanillaElevator.RaiseSafely(ev);
            if (!ev.Allow) break;
            ElevatorManager.TrySetDestination(elevatorGroup, lvl);
            break;
        }
    }
}