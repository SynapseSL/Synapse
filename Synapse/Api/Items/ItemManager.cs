using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class ItemManager
    {
        public static ItemManager Get => Server.Get.ItemManager;

        public const int HighestItem = (int)ItemType.Coin;

        private readonly List<CustomItemInformation> customItems = new List<CustomItemInformation>();

        public ItemType GetBaseType(int id)
        {
            if (id >= 0 && id <= 35)
                return (ItemType)id;

            var item = customItems.FirstOrDefault(x => x.ID == id);
            if (item == null) throw new System.Exception("BaseItemType was requested from a CustomItem which is not registered");
            return item.BasedItemType;
        }

        public string GetName(int id)
        {
            if (id >= 0 && id <= HighestItem)
                return ((ItemType)id).ToString();

            var item = customItems.FirstOrDefault(x => x.ID == id);
            if (item == null) throw new System.Exception("Name was requested from a CustomItem which is not registered");
            return item.Name;
        }

        public CustomItemInformation GetInfo(int id) => customItems.FirstOrDefault(x => x.ID == id);

        public void RegisterCustomItem(CustomItemInformation info)
        {
            if (info.ID >= 0 && info.ID <= HighestItem)
                throw new Exception("A plugin tried to register a CustomItem with an ID of a BaseGame Item");

            if (customItems.Select(x => x.ID).Contains(info.ID))
                throw new Exception("A plugin tried to register a CustomItem with an ID which was already registered");

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
