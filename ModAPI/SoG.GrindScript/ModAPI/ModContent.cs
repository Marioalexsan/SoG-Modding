using System;
using System.Collections.Generic;

namespace SoG.Modding
{
    public static class ModContent
    {
        /// <summary> 
        /// Creates a new item using the provided builder. Shorthand for CreateItem(owner, item, null). 
        /// </summary>

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ModItemBuilder item)
        {
            return CreateItem(owner, item, null);
        }

        /// <summary> 
        /// <para> Creates a new item using the provided builders. </para>
        /// <para> A null equipment builder can be provided if the new item isn't an equipment. </para>
        /// </summary>
        /// <returns>The new item's ID, or ItemCodex.ItemTypes.Null if creation failed.</returns>

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ModItemBuilder item, ModEquipBuilder equip)
        {
            if (item == null || owner == null)
            {
                Console.WriteLine("Can't create item due to owner or item being null.");
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes allocatedType = ModAllocator.AllocateItemType();

            // ModItem entry and ItemDescription need to be created before the respective EquipmentInfo
            ModItemEntry newEntry = ModLibrary.Global.ModItems[allocatedType] = new ModItemEntry()
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

            if (newEntry.equipInfo != null)
            {
                var type = (ItemCodex.ItemCategories)newEntry.equipInfo.equipType;
                itemInfo.lenCategory.Add(type);

                if (type == ItemCodex.ItemCategories.Weapon)
                {
                    switch ((newEntry.equipInfo.vanilla as WeaponInfo).enWeaponCategory)
                    {
                        case WeaponInfo.WeaponCategory.OneHanded:
                            itemInfo.lenCategory.Add(ItemCodex.ItemCategories.OneHandedWeapon); break;
                        case WeaponInfo.WeaponCategory.TwoHanded:
                            itemInfo.lenCategory.Add(ItemCodex.ItemCategories.TwoHandedWeapon); break;
                    }
                }
            }

            Ui.AddMiscText("Items", itemInfo.sNameLibraryHandle, itemInfo.sFullName, MiscTextTypes.GenericItemName);
            Ui.AddMiscText("Items", itemInfo.sDescriptionLibraryHandle, itemInfo.sDescription, MiscTextTypes.GenericItemDescription);

            return allocatedType;
        }
    }
}
