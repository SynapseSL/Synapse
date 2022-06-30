using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map;
using UnityEngine;

namespace Synapse3.SynapseModule;

#if DEBUG
public class DebugService : Service
{
    private PlayerEvents _player;

    public DebugService(PlayerEvents player)
    {
        _player = player;
    }

    public override void Enable()
    {
        _player.KeyPress.Subscribe(OnKeyPress);
    }

    private void OnKeyPress(KeyPressEvent ev)
    {
        switch (ev.KeyCode)
        {
            case KeyCode.Alpha1:
                NeuronLogger.For<Synapse>().Warn($"All Items: {ev.Player.Inventory.Items.Count}");
                break;
            
            case KeyCode.Alpha2:
                foreach (var item in Synapse.Get<ItemService>()._allItems)
                {
                    if (item.Value == null)
                    {
                        NeuronLogger.For<Synapse>().Warn($"{item.Key} is null");
                        continue;
                    }
                    
                    NeuronLogger.For<Synapse>().Warn($"{item.Key} exist ID: {item.Value.ID} State: {item.Value.State}");
                }
                break;
            
            case KeyCode.Alpha3:
                foreach (var item in Synapse.Get<ItemService>().AllItems)
                {
                    item.EquipItem(ev.Player);
                }
                break;
            
            case KeyCode.Alpha4:
                Synapse.Get<RoomService>().SpawnCustomRoom(100, ev.Player.Position);
                break;
            
            case KeyCode.Alpha5:
                NeuronLogger.For<Synapse>().Warn(ev.Player.Room.Name);
                break;
            
            case KeyCode.Alpha6:
                Synapse.Get<CassieService>().AnnounceScpDeath("056", ScpContainmentType.ClassD, "Unknown", 0.3f, 0.2f,
                    CassieSettings.Glitched, CassieSettings.DisplayText, CassieSettings.Noise);
                break;
        }
    }
}
#endif