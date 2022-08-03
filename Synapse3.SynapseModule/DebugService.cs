using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;
    private MapEvents _map;
    private RoundEvents _round;
    private ItemEvents _item;
    private SynapseCommandService _commandService;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, SynapseCommandService commandService)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _commandService = commandService;
    }

    public override void Enable()
    {
        _player.Ban.Subscribe(BasicInteract);
        _player.Damage.Subscribe(BasicInteract);
        _player.Shoot.Subscribe(BasicInteract);
        
        _map.Scp914Upgrade.Subscribe(Upgrade);
        _player.DoorInteract.Subscribe(OnDoor);
        _map.TriggerTesla.Subscribe(Tesla);
        _player.KeyPress.Subscribe(OnKeyPress);
        _round.SelectTeam.Subscribe(SelectTeam);
        _round.SpawnTeam.Subscribe(SpawnTeam);
        
        _item.KeyCardInteract.Subscribe(KeyCardItem);
        _item.BasicInteract.Subscribe(BasicItem);

        _player.Death.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} {ev.DamageType} {ev.LastTakenDamage}");
        });
        _player.Damage.Subscribe(ev =>
            NeuronLogger.For<Synapse>().Warn($"Damage: {ev.Player.NickName} {ev.Damage} {ev.DamageType}"));
    }

    private void BasicInteract(PlayerInteractEvent ev)
    {
        
    }

    private void Tesla(TriggerTeslaEvent ev)
    {
        if (ev.Player.RoleType == RoleType.ClassD) ev.Allow = false;
    }

    public override void Disable()
    {
        _map.Scp914Upgrade.Unsubscribe(Upgrade);
        _player.KeyPress.Unsubscribe(OnKeyPress);
    }

    private void OnDoor(DoorInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Door Interact");
    }

    private void KeyCardItem(KeyCardInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Keycard Use State: " + ev.State);
    }
    private void BasicItem(BasicItemInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Basic Item Use State: " + ev.State);
    }

    private void SelectTeam(SelectTeamEvent ev)
    {
        ev.TeamId = 15;
        NeuronLogger.For<Synapse>().Warn("Team Selected");
    }

    private void SpawnTeam(SpawnTeamEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("SpawnTeam: " + ev.TeamId);
    }

    private void Upgrade(Scp914UpgradeEvent ev)
    {
        ev.MoveItems = false;
        ev.MovePlayers = false;
    }

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                break;
        }
    }
}
#endif