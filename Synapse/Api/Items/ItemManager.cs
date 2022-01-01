using Synapse.Api.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class ItemManager
    {
        public static ItemManager Get => Server.Get.ItemManager;

        public const int HighestItem = (int)ItemType.SCP2176;

        private readonly List<CustomItemInformation> customItems = new List<CustomItemInformation>();

        public ItemType GetBaseType(int id)
        {
            if (id >= 0 && id <= 35)
                return (ItemType)id;

            if (!IsIDRegistered(id)) throw new SynapseItemNotFoundException("The BaseType was requested from an not registered Item ID", id);

            var item = customItems.FirstOrDefault(x => x.ID == id);
            return item.BasedItemType;
        }

        public string GetName(int id)
        {
            if (id >= 0 && id <= HighestItem)
                return ((ItemType)id).ToString();

            if (!IsIDRegistered(id)) throw new SynapseItemNotFoundException("The name was requested from an not registered Item ID", id);

            var item = customItems.FirstOrDefault(x => x.ID == id);
            return item.Name;
        }

        public CustomItemInformation GetInfo(int id) => customItems.FirstOrDefault(x => x.ID == id);

        public void RegisterCustomItem(CustomItemInformation info)
        {
            if (info.ID >= 0 && info.ID <= HighestItem)
                throw new SynapseItemAlreadyRegisteredException("A Item was registered with an ID of a Vanilla Item", info);

            if (customItems.Select(x => x.ID).Contains(info.ID))
                throw new SynapseItemAlreadyRegisteredException("A Item was registered with an already registered Item ID", info);

            customItems.Add(info);
        }

        public bool IsIDRegistered(int id)
        {
            if (id >= 0 && id <= HighestItem) return true;
            if (customItems.Any(x => x.ID == id)) return true;
            return false;
        }
    }
}
