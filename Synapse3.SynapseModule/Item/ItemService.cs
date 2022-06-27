using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map;
using Synapse3.SynapseModule.Map.Objects;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Item;

public class ItemService : Service
{
    public const int HighestItem = (int)ItemType.ParticleDisruptor;

    private RoundEvents _round;
    private readonly List<ItemInformation> _items = new();
    private readonly Dictionary<ItemType, SchematicConfiguration> overridenVanillaItems = new();

    public ItemService(RoundEvents round)
    {
        _round = round;
        round.RoundRestart.Subscribe(Clear);
    }
    
    public ReadOnlyCollection<ItemInformation> CustomItemInformation => _items.AsReadOnly();
    internal Dictionary<ushort, SynapseItem> _allItems { get; } = new ();

    public ReadOnlyCollection<SynapseItem> AllItems =>
        _allItems.Select(x => x.Value).Where(x => x is not null).ToList().AsReadOnly();

    public SynapseItem GetSynapseItem(ushort serial)
    {
        if (!_allItems.ContainsKey(serial))
        {
            NeuronLogger.For<Synapse>().Warn("If this message appears exists a Item that is not registered. Please report this bug in our Discord as detailed as possible");
            return SynapseItem.None;
        }
        return _allItems[serial];
    }
    
    public bool RegisterItem(ItemInformation info)
    {
        if (IsIdRegistered(info.ID)) return false;
        
        _items.Add(info);
        return true;
    }

    public bool UnRegisterItem(int id)
    {
        var info = GetInfo(id);
        if (info == null) return false;

        return _items.Remove(info);
    }

    public void SetVanillaItemSchematic(ItemType type, SchematicConfiguration configuration)
        => overridenVanillaItems[type] = configuration;

    public SchematicConfiguration GetSchematicConfiguration(int id)
    {
        if (id is >= 0 and <= HighestItem)
        {
            return overridenVanillaItems.FirstOrDefault(x => x.Key == (ItemType)id).Value;
        }

        var info = GetInfo(id);
        if (info == null || info.SchematicID < 0) return null;
        return Synapse.Get<SchematicService>().GetConfiguration(info.SchematicID);
    }
    
    public bool IsIdRegistered(int id)
        => id is >= 0 and <= HighestItem || _items.Any(x => x.ID == id);

    public ItemType GetBaseType(int id)
    {
        if (id is >= 0 and <= HighestItem) return (ItemType)id;

        if (!IsIdRegistered(id)) return ItemType.None;

        return GetInfo(id)?.BasedItemType ?? ItemType.None;
    }

    public string GetName(int id)
    {
        if (id is >= 0 and <= HighestItem) return ((ItemType)id).ToString();

        if (!IsIdRegistered(id)) return "";

        return GetInfo(id)?.Name ?? "";
    }
    
    private ItemInformation GetInfo(int id) => _items.FirstOrDefault(x => x.ID == id);

    private void Clear(RoundRestartEvent ev)
    {
        _allItems.Clear();
    }
}