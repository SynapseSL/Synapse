using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuron.Core;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Map.Schematic;

namespace Synapse3.SynapseModule.Item;

public class ItemService : Service
{
    public const int HighestItem = (int)ItemType.ParticleDisruptor;

    private readonly IKernel _kernel;
    private readonly RoundEvents _round;
    private readonly Synapse _synapseModule;
    
    private readonly List<ItemAttribute> _items = new();
    private readonly Dictionary<ItemType, SchematicConfiguration> overridenVanillaItems = new();

    public ItemService(RoundEvents round, IKernel kernel, Synapse synapseModule)
    {
        _kernel = kernel;
        _round = round;
        _synapseModule = synapseModule;
    }

    public override void Enable()
    {
        _round.Restart.Subscribe(Clear);
        
        while (_synapseModule.ModuleItemBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleItemBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public override void Disable()
    {
        _round.Restart.Unsubscribe(Clear);
    }

    public ReadOnlyCollection<ItemAttribute> CustomItemInformation => _items.AsReadOnly();
    internal Dictionary<ushort, SynapseItem> _allItems { get; } = new ();

    public ReadOnlyCollection<SynapseItem> AllItems =>
        _allItems.Select(x => x.Value).Where(x => x is not null).ToList().AsReadOnly();

    /// <summary>
    /// Returns the SynapseItem with that Serial or SynapseIte.None
    /// </summary>
    public SynapseItem GetSynapseItem(ushort serial)
    {
        if (!_allItems.ContainsKey(serial))
        {
            NeuronLogger.For<Synapse>().Warn("If this message appears exists a Item that is not registered. Please report this bug in our Discord as detailed as possible");
            return SynapseItem.None;
        }
        return _allItems[serial];
    }

    /// <summary>
    /// Creates and registers the CustomItemHandler and binds it to the kernel
    /// </summary>
    public bool CreateAndRegisterItemHandler(ItemAttribute info, Type handlerType)
    {
        var handler = (CustomItemHandler)_kernel.GetSafe(handlerType);
        _kernel.Bind(handlerType).ToConstant(handler).InSingletonScope();
        handler.Attribute = info;
        return RegisterItem(info);
    }
    
    /// <summary>
    /// Registers the CustomItem
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool RegisterItem(ItemAttribute info)
    {
        if (IsIdRegistered(info.Id)) return false;
        
        _items.Add(info);
        return true;
    }

    /// <summary>
    /// Removes the CustomItem so that it can no longer be spawned
    /// </summary>
    public bool UnRegisterItem(uint id)
    {
        var info = GetInfo(id);
        if (info == null) return false;

        return _items.Remove(info);
    }

    /// <summary>
    /// Sets the Schematic Look for an Vanilla Item
    /// </summary>
    public void SetVanillaItemSchematic(ItemType type, SchematicConfiguration configuration)
        => overridenVanillaItems[type] = configuration;

    /// <summary>
    /// Returns the Schematic Configuration registered with this ID
    /// </summary>
    public SchematicConfiguration GetSchematicConfiguration(uint id)
    {
        switch (id)
        {
            case uint.MaxValue:
                return null;
            
            case >= 0 and <= HighestItem:
                return overridenVanillaItems.FirstOrDefault(x => x.Key == (ItemType)id).Value;
        }

        var info = GetInfo(id);
        if (info == null || info.SchematicID < 0) return null;
        return Synapse.Get<SchematicService>().GetConfiguration(info.SchematicID);
    }
    
    public bool IsIdRegistered(uint id)
        => id is >= 0 and <= HighestItem || _items.Any(x => x.Id == id);

    public ItemType GetBaseType(uint id)
    {
        if (id is >= 0 and <= HighestItem) return (ItemType)id;

        if (!IsIdRegistered(id)) return ItemType.None;

        return GetInfo(id)?.BasedItemType ?? ItemType.None;
    }

    public string GetName(uint id)
    {
        if (id is >= 0 and <= HighestItem) return ((ItemType)id).ToString();

        if (!IsIdRegistered(id)) return "";

        return GetInfo(id)?.Name ?? "";
    }
    
    private ItemAttribute GetInfo(uint id) => _items.FirstOrDefault(x => x.Id == id);

    private void Clear(RoundRestartEvent ev)
    {
        foreach (var item in _allItems)
        {
            item.Value?.OnDestroy();
        }
        _allItems.Clear();
    }

    internal void LoadBinding(SynapseItemBinding binding)
    {
        CreateAndRegisterItemHandler(binding.Info, binding.HandlerType);
    }
}