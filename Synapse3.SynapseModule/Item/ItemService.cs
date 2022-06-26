using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Item;

public class ItemService : Service
{
    public const int HighestItem = (int)ItemType.ParticleDisruptor;
    
    private readonly List<ItemInformation> _items = new();
    private readonly Dictionary<ItemType, SchematicConfiguration> overridenVanillaItems = new();

    public ReadOnlyCollection<ItemInformation> Items => _items.AsReadOnly();

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

    public SchematicConfiguration GetSchematicConfiguration(int id)
    {
        return null;
    }
    
    public bool IsIdRegistered(int id)
        => id is >= 0 and <= HighestItem || _items.Any(x => x.ID == id);
    
    
    private ItemInformation GetInfo(int id) => _items.FirstOrDefault(x => x.ID == id);
}