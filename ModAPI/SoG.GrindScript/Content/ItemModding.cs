using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    public class ItemModding : ModdingLogic
    {
        public ItemModding(GrindScript modAPI)
            : base(modAPI) { }

        /// <summary> 
        /// Creates an item for each of the ItemConfigs provided. 
        /// </summary>

        public void CreateItems(IEnumerable<ItemConfig> cfgs)
        {
            foreach (var cfg in cfgs)
                CreateItem(cfg);
        }

        /// <summary> 
        /// Creates an item from the given ItemConfig.
        /// </summary>
        /// <returns> 
        /// The new item's <see cref="ItemCodex.ItemTypes"/> identifier. If creation failed, <see cref="ItemCodex.ItemTypes.Null"/> is returned. 
        /// </returns>

        public ItemCodex.ItemTypes CreateItem(ItemConfig cfg)
        {
            BaseScript owner = _modAPI.CurrentModContext;
            if (cfg == null || owner == null)
            {
                ModGlobals.Log.Error("Can't create item because owner or itemBuilder is null.");
                return ItemCodex.ItemTypes.Null;
            }

            ItemCodex.ItemTypes gameID = _modAPI.Allocator.ItemID.Allocate();

            ModItemEntry entry = new ModItemEntry(owner, gameID, cfg.ModID)
            {
                ItemShadowPath = cfg.ShadowPath,
                Manager = cfg.Manager,
                ItemResourcePath = cfg.IconPath,
                EquipResourcePath = cfg.EquipResourcePath
            };

            _modAPI.Library.Items[gameID] = owner.Library.Items[gameID] = entry;

            ItemDescription itemData = entry.ItemData = new ItemDescription()
            {
                enType = gameID,
                sFullName = cfg.Name,
                sDescription = cfg.Description,
                sNameLibraryHandle = $"Item_{(int)gameID}_Name",
                sDescriptionLibraryHandle = $"Item_{(int)gameID}_Description",
                sCategory = "",
                iInternalLevel = cfg.SortingValue,
                byFancyness = Math.Min((byte)1, Math.Max(cfg.Fancyness, (byte)3)),
                iValue = cfg.Value,
                iOverrideBloodValue = cfg.BloodValue,
                fArcadeModeCostModifier = cfg.ArcadeValueModifier,
                lenCategory = new HashSet<ItemCodex.ItemCategories>(cfg.Categories)
            };

            EquipmentType typeToUse = Enum.IsDefined(typeof(EquipmentType), cfg.EquipType) ? cfg.EquipType : EquipmentType.None;

            EquipmentInfo equipData = null;
            switch (typeToUse)
            {
                case EquipmentType.None:
                    break;
                case EquipmentType.Facegear:
                    FacegearInfo faceData = (equipData = new FacegearInfo(gameID)) as FacegearInfo;

                    Array.Copy(cfg.FacegearOverHair, faceData.abOverHair, 4);
                    Array.Copy(cfg.FacegearOverHat, faceData.abOverHat, 4);
                    Array.Copy(cfg.FacegearOffsets, faceData.av2RenderOffsets, 4);

                    break;
                case EquipmentType.Hat:
                    HatInfo hatData = (equipData = new HatInfo(gameID) { bDoubleSlot = cfg.HatDoubleSlot }) as HatInfo;

                    InitializeSet(hatData.xDefaultSet, cfg.DefaultSet);
                    foreach (var kvp in cfg.AltSets)
                        InitializeSet(hatData.denxAlternateVisualSets[kvp.Key] = new HatInfo.VisualSet(), kvp.Value);
                    
                    break;
                case EquipmentType.Weapon:
                    WeaponInfo weaponData = new WeaponInfo(cfg.EquipResourcePath, gameID, cfg.WeaponType)
                    {
                        enWeaponCategory = cfg.WeaponType,
                        enAutoAttackSpell = WeaponInfo.AutoAttackSpell.None
                    };
                    equipData = weaponData;

                    if (cfg.WeaponType == WeaponInfo.WeaponCategory.OneHanded)
                    {
                        weaponData.iDamageMultiplier = 90;
                        if (cfg.MagicWeapon)
                            weaponData.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic1H;
                    }
                    else if (cfg.WeaponType == WeaponInfo.WeaponCategory.TwoHanded)
                    {
                        weaponData.iDamageMultiplier = 125;
                        if (cfg.MagicWeapon)
                            weaponData.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic2H;
                    }
                    break;
                default:
                    equipData = new EquipmentInfo(cfg.EquipResourcePath, gameID);
                    break;
            }

            if (cfg.EquipType != EquipmentType.None)
            {
                equipData.deniStatChanges = new Dictionary<EquipmentInfo.StatEnum, int>(cfg.Stats);
                equipData.lenSpecialEffects.AddRange(cfg.Effects);

                if (cfg.EquipType == EquipmentType.Hat)
                {
                    var altResources = entry.HatAltSetResourcePaths = new Dictionary<ItemCodex.ItemTypes, string>();
                    foreach (var set in cfg.AltSets)
                        altResources.Add(set.Key, set.Value.Resource);
                }
            }

            entry.EquipData = equipData;

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

            itemData.lenCategory.ExceptWith(toSanitize);

            if (equipData != null)
            {
                ItemCodex.ItemCategories type = (ItemCodex.ItemCategories)cfg.EquipType;
                itemData.lenCategory.Add(type);

                if (type == ItemCodex.ItemCategories.Weapon)
                {
                    switch ((equipData as WeaponInfo).enWeaponCategory)
                    {
                        case WeaponInfo.WeaponCategory.OneHanded:
                            itemData.lenCategory.Add(ItemCodex.ItemCategories.OneHandedWeapon); break;
                        case WeaponInfo.WeaponCategory.TwoHanded:
                            itemData.lenCategory.Add(ItemCodex.ItemCategories.TwoHandedWeapon); break;
                    }
                }
            }

            TextModding.AddMiscText("Items", itemData.sNameLibraryHandle, itemData.sFullName, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Items", itemData.sDescriptionLibraryHandle, itemData.sDescription, MiscTextTypes.GenericItemDescription);

            if (owner.Library.Items.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                ModGlobals.Log.Error($"Mod {owner.GetType().Name} has two or more items with the uniqueID {cfg.ModID}!");
                ModGlobals.Log.Error($"This is likely to break loading, saving, and many other things!");
            }

            return gameID;

            void InitializeSet(HatInfo.VisualSet set, ItemConfig.VSetInfo desc)
            {
                Array.Copy(desc.HatUnderHair, set.abUnderHair, 4);
                Array.Copy(desc.HatBehindPlayer, set.abBehindCharacter, 4);
                Array.Copy(desc.HatOffsets, set.av2RenderOffsets, 4);
                set.bObstructsSides = desc.ObstructHairSides;
                set.bObstructsTop = desc.ObstructHairTop;
                set.bObstructsBottom = desc.ObstructHairBottom;
            }
        }

        /// <summary>
        /// Adds a new crafting recipe.
        /// </summary>

        public void AddRecipe(ItemCodex.ItemTypes result, Dictionary<ItemCodex.ItemTypes, ushort> ingredients)
        {
            if (ingredients == null)
            {
                ModGlobals.Log.Warn("Can't add recipe because ingredient dictionary is null!");
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

            ModGlobals.Log.Info($"Added recipe for item {result}!");
        }

        /// <summary>
        /// Gets an ItemType previously defined by a mod.
        /// If nothing is found, ItemCodex.ItemTypes.Null is returned.
        /// </summary>

        public ItemCodex.ItemTypes GetItemType(BaseScript owner, string uniqueID)
        {
            var entry = _modAPI.Library.Items.Values.FirstOrDefault(x => x.Owner == owner && x.ModID == uniqueID);

            return entry?.GameID ?? ItemCodex.ItemTypes.Null;
        }
    }
}
