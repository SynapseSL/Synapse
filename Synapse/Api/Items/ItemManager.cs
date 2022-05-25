using Synapse.Api.CustomObjects;
using Synapse.Api.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class ItemManager
    {
        public static ItemManager Get
            => Server.Get.ItemManager;

        public const int HighestItem = (int)ItemType.ParticleDisruptor;

        private readonly List<CustomItemInformation> _customItems;

        private readonly Dictionary<ItemType, SynapseSchematic> _overridenVanillaSchematics;

        public ItemManager()
        {
            _customItems = new List<CustomItemInformation>();
            _overridenVanillaSchematics = new Dictionary<ItemType, SynapseSchematic>();
        }

        public ItemType GetBaseType(int id)
        {
            if (id >= 0 && id <= HighestItem)
                return (ItemType)id;

            if (!IsIDRegistered(id))
                throw new SynapseItemNotFoundException("The BaseType was requested from an not registered Item ID", id);

            var item = _customItems.FirstOrDefault(x => x.ID == id);
            return item.BasedItemType;
        }

        public string GetName(int id)
        {
            if (id >= 0 && id <= HighestItem)
                return ((ItemType)id).ToString();

            if (!IsIDRegistered(id))
                throw new SynapseItemNotFoundException("The name was requested from an not registered Item ID", id);

            var item = _customItems.FirstOrDefault(x => x.ID == id);
            return item.Name;
        }

        public SynapseSchematic GetSchematic(int id)
        {
            if (id >= 0 && id <= HighestItem)
            {
                if (_overridenVanillaSchematics.ContainsKey((ItemType)id))
                    return _overridenVanillaSchematics.FirstOrDefault(x => x.Key == (ItemType)id).Value;
                else
                    return null;
            }

            var item = _customItems.FirstOrDefault(x => x.ID == id);
            return item is null || item.SchematicID < 0 ? null : SchematicHandler.Get.GetSchematic(item.SchematicID);
        }

        public CustomItemInformation GetInfo(int id)
            => _customItems.FirstOrDefault(x => x.ID == id);

        public void RegisterCustomItem(CustomItemInformation info)
        {
            if (info.ID >= 0 && info.ID <= HighestItem)
                throw new SynapseItemAlreadyRegisteredException("A Item was registered with an ID of a Vanilla Item", info);

            if (_customItems.Select(x => x.ID).Contains(info.ID))
                throw new SynapseItemAlreadyRegisteredException("A Item was registered with an already registered Item ID", info);

            _customItems.Add(info);
        }

        public void SetSchematicForVanillaItem(ItemType item, SynapseSchematic schematic)
            => _overridenVanillaSchematics[item] = schematic;

        public bool IsIDRegistered(int id)
            => (id >= 0 && id <= HighestItem) || _customItems.Any(x => x.ID == id);
    }
}
