using System.Collections.Generic;
using InventorySystem.Items.MicroHID;
using MEC;
using Mirror;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using PlayerStatsSystem;
using Respawning;
using Respawning.NamingRules;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Dummy;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Player;
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
    private SynapseCommandService _commandService;

    public DebugService(PlayerEvents player, MapEvents map, RoundEvents round, ItemEvents item, ScpEvents scp,
        SynapseCommandService commandService, ServerEvents server)
    {
        _player = player;
        _map = map;
        _round = round;
        _item = item;
        _server = server;
        _commandService = commandService;
        _scp = scp;
    }

    public override void Enable()
    {
        _player.DoorInteract.Subscribe(OnDoor);
        _player.KeyPress.Subscribe(OnKeyPress);
        _round.SelectTeam.Subscribe(SelectTeam);
        _round.SpawnTeam.Subscribe(SpawnTeam);
        
        _item.KeyCardInteract.Subscribe(KeyCardItem);
        _item.BasicInteract.Subscribe(BasicItem);
        
        _item.Shoot.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Shoot {ev.Player.NickName} {ev.Target?.NickName} {ev.Item.ItemType}");
        });

        _scp.Revive.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>()
                .Warn($"Revive {ev.Scp049.NickName} {ev.HumanToRevive.NickName} {ev.Ragdoll.RoleType} Finish: {ev.FinishRevive}");
        });
        
        _item.ThrowGrenade.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Throw {ev.State}");
        });
        
        _item.MicroUse.Subscribe(ev =>
        {
            if (ev.MicroState == HidState.PoweringUp)
            {
                ev.AllowChangingState = false;
                ev.MicroState = HidState.Firing;
            }
        });

        _player.Death.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} {ev.DamageType} {ev.LastTakenDamage} {ev.DeathMessage ?? "NONE"}");
        });
        
        _player.WalkOnHazard.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"HAZARD {ev.Player.NickName}");
        });
        
        _player.StartWorkStation.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"WorkStation {ev.Player.NickName}");
        });
        _player.Damage.Subscribe(ev =>
            NeuronLogger.For<Synapse>().Warn($"Damage: {ev.Player.NickName} {ev.Damage} {ev.DamageType}"));
            
        _player.FallingIntoAbyss.Subscribe(ev =>
                NeuronLogger.For<Synapse>().Warn($"{ev.Player.NickName} falled into an abyss"));
        
        _server.PreAuthentication.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn($"Pre Auth {ev.UserId}");
        });
        
        _scp.Scp049Attack.Subscribe(ScpEvent);
        _scp.Scp0492Attack.Subscribe(ScpEvent);
        _scp.Scp173Attack.Subscribe(ScpEvent);
        _scp.Scp096Attack.Subscribe(ScpEvent);
        _scp.Scp939Attack.Subscribe(ScpEvent);
        _scp.Scp106Attack.Subscribe(ScpEvent);

        _scp.ContainScp079.Subscribe(ev => NeuronLogger.For<Synapse>().Warn("Contain 079: " + ev.Status));
        _scp.SwitchCamera.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn("Switch Cam " + ev.Scp079.NickName + " " + ev.Camera.CameraID);
            ev.Allow = Random.Range(0, 2) == 1;
        });
        
        _scp.Scp079DoorInteract.Subscribe(ev =>
        {
            NeuronLogger.For<Synapse>().Warn("079 Door");
        });

        _scp.Revive.Subscribe(ev => ev.Allow = false);

        Synapse.Get<SynapseObjectEvents>().ButtonPressed
            .Subscribe(ev =>
            {
                NeuronLogger.For<Synapse>().Warn("Button Pressed " + ev.ButtonId);
                ev.Player.SendBroadcast("You pressed me!", 5);
            });
    }

    private void ScpEvent(ScpAttackEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn($"{ev.ScpAttackType} {ev.Damage} {ev.Scp.NickName} | {ev.Victim.NickName}");
        //ev.Allow = false;
    }
    
    private void OnDoor(DoorInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Door Interact");
    }

    private void KeyCardItem(KeyCardInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Keycard Use State: " + ev.State + " " + ev.Allow);
    }
    private void BasicItem(BasicItemInteractEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("Basic Item Use State: " + ev.State);
    }

    private void SelectTeam(SelectTeamEvent ev)
    {
        //ev.TeamId = 15;
        NeuronLogger.For<Synapse>().Warn("Team Selected " + ev.TeamId);
    }

    private void SpawnTeam(SpawnTeamEvent ev)
    {
        NeuronLogger.For<Synapse>().Warn("SpawnTeam: " + ev.TeamId);
    }
    
    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                var rag = new SynapseRagdoll(ev.Player.RoleType, "1", ev.Player.Position, ev.Player.Rotation,
                    Vector3.one, "Dimenzio the Second", ev.Player);
                Timing.CallDelayed(3f, () =>
                {
                    rag.Info = new SynapseRagdoll.SynapseRagDollInfo("2", "New Dimenzio", RoleType.Scientist);
                });
                break;
            
            case KeyCode.Alpha2:
                rag = new SynapseRagdoll(ev.Player.RoleType, "3", ev.Player.Position, ev.Player.Rotation,
                    Vector3.one, "Dimenzio the Second", ev.Player,false,uint.MaxValue,false);
                Timing.CallDelayed(8f, () =>
                {
                    rag.VisibleInfoCondition[x => true] =
                        new SynapseRagdoll.SynapseRagDollInfo(new WarheadDamageHandler(), "New Dimenzio the second",
                            RoleType.Scp096);
                    rag.UpdateInfo();
                });
                break;

            case KeyCode.Alpha3:
                ev.Player.SendNetworkMessage(Synapse.Get<MirrorService>().GetCustomVarMessage(
                    ServerConfigSynchronizer.Singleton,
                    writer =>
                    {
                        writer.WriteUInt64(4); //Which SyncObject will be updated in this case 1 2 or 4 or a combination of those
                        
                        //SyncList Specific
                        writer.WriteUInt32(1); //The amount of changes
                        writer.WriteByte((byte)SyncList<byte>.Operation.OP_ADD);
                        writer.Write(new ServerConfigSynchronizer.PredefinedBanTemplate()
                        {
                            Duration = 1,
                            Reason = "test",
                            DurationNice = "test duration"
                        });
                    }, false));
                break;
            
            case KeyCode.Alpha4:
                ev.Player.SendNetworkMessage(new SyncedStatMessages.StatMessage()
                {
                    NetId = ev.Player.NetworkIdentity.netId,
                    SyncedValue = 15,
                    SyncId = 2
                });
                break;

            case KeyCode.Alpha9:
                for (int i = 0; i < ev.Player.PlayerEffectsController._allEffects.Length; i++)
                {
                    var effect = ev.Player.PlayerEffectsController._allEffects[i];
                    Logger.Warn(i + " - " + effect.GetType());
                }
                break;
        }
    }

    private IEnumerator<float> HAND(SynapsePlayer player)
    {
        var pos = player.Position;
        pos.y += 10f;
        var dummy = new SynapseDummy(pos, player.Rotation, player.RoleType, "Hand Spawner");
        dummy.Scale = Vector3.zero;
        for (;;)
        {
            player.SendFakeEffectIntensityFor(dummy.Player, Effect.SeveredHands, 1);
            yield return Timing.WaitForOneFrame;
            player.SendFakeEffectIntensityFor(dummy.Player, Effect.SeveredHands, 0);
            yield return Timing.WaitForOneFrame;
        }
    }
}
#endif
