using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding;

namespace SoG.Modding
{
    public static partial class GrindScript
    {
        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ModItemInfoBuilder item)
        {
            return CreateItem(owner, item, null);
        }

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ModItemInfoBuilder item, ModEquipInfoBuilder equip)
        {
            if (item == null || owner == null)
            {
                Console.WriteLine("Can't create item due to owner / item being null.");
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes allocatedType = ModLibrary.AllocateItemType();


            // ModItem entry and ItemDescription need to be created before the respective EquipmentInfo
            ModItem newEntry = GlobalLib.ModItems[allocatedType] = new ModItem()
            {
                owner = owner,
                type = allocatedType,
                itemInfo = item.Build(allocatedType),
            };
            newEntry.equipInfo = equip?.Build(allocatedType);

            ItemDescription itemInfo = newEntry.itemInfo.vanilla;

            var toSanitize = new List<ItemCodex.ItemCategories>
            {
                ItemCodex.ItemCategories.OneHandedWeapon,
                ItemCodex.ItemCategories.TwoHandedWeapon,
                ItemCodex.ItemCategories.Weapon,
                ItemCodex.ItemCategories.Shield,
                ItemCodex.ItemCategories.Armor,
                ItemCodex.ItemCategories.Hat,
                ItemCodex.ItemCategories.Accessory,
                ItemCodex.ItemCategories.Shoes,
                ItemCodex.ItemCategories.Facegear
            };

            foreach (var toRemove in toSanitize)
                itemInfo.lenCategory.Remove(toRemove);

            if (newEntry.equipInfo != null && toSanitize.Contains(newEntry.equipInfo.equipCategory))
            {
                var wantedCategory = newEntry.equipInfo.equipCategory;
                itemInfo.lenCategory.Add(wantedCategory);

                switch (wantedCategory)
                {
                    case ItemCodex.ItemCategories.OneHandedWeapon:
                    case ItemCodex.ItemCategories.TwoHandedWeapon:
                        itemInfo.lenCategory.Add(ItemCodex.ItemCategories.Weapon);
                        break;
                }
            }

            Ui.AddMiscText("Items", itemInfo.sNameLibraryHandle, itemInfo.sFullName, MiscTextTypes.GenericItemName);
            Ui.AddMiscText("Items", itemInfo.sDescriptionLibraryHandle, itemInfo.sDescription, MiscTextTypes.GenericItemDescription);

            return allocatedType;
        }
    }
}
