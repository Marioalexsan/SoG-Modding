using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public static class ItemModding
    {
        /// <summary>
        /// Helper method that calls <see cref="CreateItem(BaseScript, ItemConfig)"/> for each configuration.
        /// </summary>

        public static void CreateItemsFrom(BaseScript owner, IEnumerable<ItemConfig> cfgs)
        {
            foreach (var cfg in cfgs)
                CreateItem(owner, cfg);
        }

        /// <summary> 
        /// Creates a new item from the given <see cref="ItemConfig"/>. <para/>
        /// </summary>
        /// <returns> The new item's <see cref="ItemCodex.ItemTypes"/> identifier. If creation failed, <see cref="ItemCodex.ItemTypes.Null"/> is returned. </returns>

        public static ItemCodex.ItemTypes CreateItem(BaseScript owner, ItemConfig cfg)
        {
            if (cfg == null || owner == null)
            {
                GrindScript.Logger.Error("Can't create item because owner or itemBuilder is null.");
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes allocatedType = IDAllocator.NewItemType();

            ModItemEntry newEntry = new ModItemEntry()
            {
                ModID = cfg.UniqueID,
                Owner = owner,
                GameID = allocatedType,
                itemInfo = cfg.CreateModItem(allocatedType),
            };

            ModLibrary.GlobalLib.Items[allocatedType] = newEntry;
            owner.ModLib.Items[allocatedType] = newEntry;

            newEntry.equipInfo = cfg.CreateEquipInfo(allocatedType); // Must be done after ItemDescription can be retrieved by SoG

            ItemDescription itemInfo = newEntry.itemInfo.vanilla;

            HashSet<ItemCodex.ItemCategories> toSanitize = new HashSet<ItemCodex.ItemCategories>
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
            itemInfo.lenCategory.ExceptWith(toSanitize);

            if (newEntry.equipInfo != null)
            {
                ItemCodex.ItemCategories type = (ItemCodex.ItemCategories)newEntry.equipInfo.equipType;
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

            itemInfo.sNameLibraryHandle = (int)allocatedType + "_Name";
            itemInfo.sDescriptionLibraryHandle = (int)allocatedType + "_sDescription";

            TextModding.AddMiscText("Items", itemInfo.sNameLibraryHandle, itemInfo.sFullName, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Items", itemInfo.sDescriptionLibraryHandle, itemInfo.sDescription, MiscTextTypes.GenericItemDescription);

            if (owner.ModLib.Items.Values.Any(x => x != newEntry && x.ModID == cfg.UniqueID))
            {
                GrindScript.Logger.Error($"Mod {owner.GetType().Name} has two or more items with the uniqueID {cfg.UniqueID}!");
                GrindScript.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            return allocatedType;
        }

        /// <summary>
        /// Adds a new crafting recipe.
        /// </summary>

        public static void AddRecipe(ItemCodex.ItemTypes result, Dictionary<ItemCodex.ItemTypes, ushort> ingredients)
        {
            if (ingredients == null)
            {
                GrindScript.Logger.Warn("Can't add recipe because ingredient dictionary is null!");
                return;
            }

            if (!Crafting.CraftSystem.RecipeCollection.ContainsKey(result))
            {
                var kvps = new KeyValuePair<ItemDescription, ushort>[ingredients.Count];

                int index = 0;
                foreach (var kvp in ingredients)
                    kvps[index++] = new KeyValuePair<ItemDescription, ushort>(ItemCodex.GetItemDescription(kvp.Key), kvp.Value);

                ItemDescription description = ItemCodex.GetItemDescription(result);
                Crafting.CraftSystem.RecipeCollection.Add(result, new Crafting.CraftSystem.CraftingRecipe(description, kvps));
            }

            GrindScript.Logger.Info($"Added recipe for item {result}!");
        }

        /// <summary>
        /// Gets an ItemType previously defined by a mod.
        /// If nothing is found, ItemCodex.ItemTypes.Null is returned.
        /// </summary>

        public static ItemCodex.ItemTypes GetItemType(BaseScript owner, string uniqueID)
        {
            var entry = ModLibrary.GlobalLib.Items.Values.FirstOrDefault(x => x.Owner == owner && x.ModID == uniqueID);

            return entry?.GameID ?? ItemCodex.ItemTypes.Null;
        }
    }
}
