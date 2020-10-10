using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api.Items
{
    public class ItemManager
    {
        private readonly List<CustomItemInformations> customItems = new List<CustomItemInformations>();

        public ItemType GetBaseType(int id)
        {
            if (id >= 0 && id <= 35)
                return (ItemType)id;

            var item = customItems.FirstOrDefault(x => x.ID == id);
            if (item == null) throw new System.Exception("BaseItemType was request from an CustomItem which is not registered");
            return item.BasedItemType;
        }

        public string GetName(int id)
        {
            if (id >= 0 && id <= 35)
                return ((ItemType)id).ToString();

            var item = customItems.FirstOrDefault(x => x.ID == id);
            if (item == null) throw new System.Exception("Name was request from an CustomItem which is not registered");
            return item.Name;
        }

        public void RegisterCustomItem(CustomItemInformations info)
        {
            if (info.ID >= 0 && info.ID <= 35)
                throw new Exception("A Plugin tryied to register a CustomItem with an ID of a BaseGame Item");

            if (customItems.Select(x => x.ID).Contains(info.ID))
                throw new Exception("A Plugin tryied to register a CustomItem with an ID which already was registered");

            customItems.Add(info);
        }
    }
}
