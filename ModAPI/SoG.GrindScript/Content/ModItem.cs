using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using EStat = SoG.EquipmentInfo.StatEnum;

namespace SoG.Modding
{
    public static partial class SoGExtension
    {
        public static Item SpawnItem(this ItemCodex.ItemTypes enType, PlayerView xTarget)
        {
            PlayerEntity xEntity = xTarget.xEntity;

            return enType.SpawnItem(xEntity.xTransform.v2Pos, xEntity.xRenderComponent.fVirtualHeight, xEntity.xCollisionComponent.ibitCurrentColliderLayer);
        }

        public static Item SpawnItem(this ItemCodex.ItemTypes enType, Vector2 v2Pos, float fVirtualHeight, int iColliderLayer)
        {
            Vector2 v2ThrowDirection = Utility.RandomizeVector2Direction(CAS.RandomInLogic);

            return GrindScript.Game._EntityMaster_AddItem(enType, v2Pos, fVirtualHeight, iColliderLayer, v2ThrowDirection);
        }

        public static bool IsSoGItem(this ItemCodex.ItemTypes enType)
        {
            return Enum.IsDefined(typeof(ItemCodex.ItemTypes), enType);
        }

        public static bool IsModItem(this ItemCodex.ItemTypes enType)
        {
            return enType >= ModLibrary.ItemTypesStart && enType < ModLibrary.ItemTypesNext;
        }
    }

    internal class ModItem
    {
        public BaseScript owner;

        public ItemCodex.ItemTypes type;

        public ModItemInfo itemInfo;

        public ModEquipInfo equipInfo;
    }

    internal class ModItemInfo
    {
        public ItemDescription vanilla;

        public string shadowToUse = "hartass02";

        public string resourceToUse = "";

        public ContentManager managerToUse;
    }

    internal class ModEquipInfo
    {
        public ItemCodex.ItemTypes type;

        public string resourceToUse = "";

        public EquipmentInfo vanilla;

        public ContentManager managerToUse;

        public Dictionary<ItemCodex.ItemTypes, string> hatAltSetResources;

        public ItemCodex.ItemCategories equipCategory;
    }

    public class ModItemInfoBuilder
    {
        private string _name;
        private string _description;
        private string _displayTexPath;
        private string _shadowTexPath;

        private int _value;
        private int _bloodValue;
        private float _arcadeValueMod;

        private ushort _levelForBestSort;
        private byte _fancyness = 1;

        private ContentManager _managerToUse;
        private ItemCodex.ItemCategories[] _categories = new ItemCodex.ItemCategories[0];

        public ModItemInfoBuilder() { }

        public ModItemInfoBuilder(string name, string description)
        {
            Texts(name, description);
        }

        public ModItemInfoBuilder Texts(string name, string description)
        {
            _name = name;
            _description = description;
            return this;
        }

        public ModItemInfoBuilder Resources(ContentManager manager, string displayTexPath, string shadowTexPath)
        {
            _managerToUse = manager;
            _displayTexPath = displayTexPath;
            _shadowTexPath = shadowTexPath;
            return this;
        }

        public ModItemInfoBuilder Categories(params ItemCodex.ItemCategories[] categories)
        {
            _categories = categories;
            return this;
        }

        public ModItemInfoBuilder Value(int value, float arcadeModifier = 1f, int bloodCost = 0)
        {
            _value = value;
            _arcadeValueMod = arcadeModifier;
            _bloodValue = bloodCost;
            return this;
        }

        public ModItemInfoBuilder Level(ushort levelForBestSort)
        {
            _levelForBestSort = levelForBestSort;
            return this;
        }

        public ModItemInfoBuilder Fancyness(byte fancyness)
        {
            _fancyness = fancyness;
            return this;
        }

        internal ModItemInfo Build(ItemCodex.ItemTypes allocatedType)
        {
            string entry = _name.Replace(" ", "");

            ItemDescription itemInfo = new ItemDescription()
            {
                sFullName = _name,
                sDescription = _description,
                sNameLibraryHandle = entry + "_Name",
                sDescriptionLibraryHandle = entry + "_Description",
                sCategory = "",
                iInternalLevel = _levelForBestSort,
                byFancyness = Math.Min((byte)1, Math.Max(_fancyness, (byte)3)),
                iValue = _value,
                iOverrideBloodValue = _bloodValue,
                enType = allocatedType
            };

            return new ModItemInfo()
            {
                vanilla = itemInfo,
                shadowToUse = _shadowTexPath,
                managerToUse = _managerToUse,
                resourceToUse = _displayTexPath
            };
        }
    }

    public class ModEquipInfoBuilder
    {
        private static readonly EStat[] statEnumArray = { 
            EStat.HP, EStat.EP, EStat.ATK, EStat.MATK, EStat.DEF, EStat.ASPD, EStat.CSPD, EStat.Crit, EStat.CritDMG, EStat.ShldHP, EStat.EPRegen, EStat.ShldRegen 
        };

        protected string _resource;
        protected ContentManager _managerToUse;
        protected Dictionary<EStat, int> _stats = new Dictionary<EStat, int>();
        protected EquipmentInfo.SpecialEffect[] _specialEffects = new EquipmentInfo.SpecialEffect[0];
        protected ItemCodex.ItemCategories _categoryForBasic = ItemCodex.ItemCategories.Accessory;

        public ModEquipInfoBuilder Resource(ContentManager manager, string resource)
        {
            _managerToUse = manager;
            _resource = resource;
            return this;
        }

        public ModEquipInfoBuilder Stats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            int[] statValueArray = { HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen };

            for (int i = 0; i < statValueArray.Length; i++)
            {
                if (statValueArray[i] == 0) _stats.Remove(statEnumArray[i]);
                else _stats.Add(statEnumArray[i], statValueArray[i]);
            }

            return this;
        }

        public ModEquipInfoBuilder SpecialEffects(params EquipmentInfo.SpecialEffect[] effects)
        {
            _specialEffects = effects;
            return this;
        }

        // This is only useful with ModEquipmentBuilder. Derived builders will override this on Build()
        public ModEquipInfoBuilder EquipmentType(ItemCodex.ItemCategories category)
        {
            _categoryForBasic = category;
            return this;
        }

        protected void BasicBuildOnto(EquipmentInfo info)
        {
            foreach (var kvp in _stats)
                if (kvp.Value != 0) info.deniStatChanges.Add(kvp.Key, kvp.Value);

            foreach (var effect in _specialEffects)
                info.lenSpecialEffects.Add(effect);
        }

        internal virtual ModEquipInfo Build(ItemCodex.ItemTypes allocatedType)
        {
            EquipmentInfo equipInfo = new EquipmentInfo(_resource, allocatedType);
            BasicBuildOnto(equipInfo);
            return new ModEquipInfo()
            {
                type = allocatedType,
                resourceToUse = _resource,
                managerToUse = _managerToUse,
                vanilla = equipInfo,
                equipCategory = _categoryForBasic
            };
        }
    }

    public class ModFacegearInfoBuilder : ModEquipInfoBuilder
    {
        private bool[] _sortsOverHair = new bool[] { true, true, true, true };
        private bool[] _sortsOverHat = new bool[] { true, true, true, true };
        private Vector2[] _renderOffsets = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

        public ModFacegearInfoBuilder RendersOverHair(bool up, bool right, bool down, bool left)
        {
            _sortsOverHair = new bool[] { up, right, down, left };
            return this;
        }

        public ModFacegearInfoBuilder RendersOverHat(bool up, bool right, bool down, bool left)
        {
            _sortsOverHat = new bool[] { up, right, down, left };
            return this;
        }

        public ModFacegearInfoBuilder RenderOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            _renderOffsets = new Vector2[] { 
                new Vector2(up.X, up.Y), new Vector2(right.X, right.Y), new Vector2(down.X, down.Y), new Vector2(left.X, left.Y) 
            };
            return this;
        }

        // Method hiding used so that this returns the derived class reference

        public new ModFacegearInfoBuilder Resource(ContentManager manager, string resource)
        {
            return (ModFacegearInfoBuilder)base.Resource(manager, resource);
        }

        public new ModFacegearInfoBuilder Stats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            return (ModFacegearInfoBuilder)base.Stats(HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen);
        }

        public new ModFacegearInfoBuilder SpecialEffects(params EquipmentInfo.SpecialEffect[] effects)
        {
            return (ModFacegearInfoBuilder)base.SpecialEffects(effects);
        }

        internal override ModEquipInfo Build(ItemCodex.ItemTypes allocatedType)
        {
            FacegearInfo equipInfo = new FacegearInfo(allocatedType);
            BasicBuildOnto(equipInfo);

            _sortsOverHair.CopyTo(equipInfo.abOverHair, 0);
            _sortsOverHat.CopyTo(equipInfo.abOverHat, 0);
            _renderOffsets.CopyTo(equipInfo.av2RenderOffsets, 0);

            _categoryForBasic = ItemCodex.ItemCategories.Facegear;
            return new ModEquipInfo()
            {
                type = allocatedType,
                resourceToUse = _resource,
                managerToUse = _managerToUse,
                vanilla = equipInfo,
                equipCategory = _categoryForBasic
            };
        }
    }

    public class ModHatInfoBuilder : ModEquipInfoBuilder
    {
        private class VSetInfo
        {
            public bool[] _sortsUnderHair = new bool[] { false, false, false, false };
            public bool[] _sortsBehindChar = new bool[] { false, false, false, false };
            public Vector2[] _renderOffsets = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };
            public bool _obstructSides = true;
            public bool _obstructTop = true;
            public bool _obstructBottom = false;
        }

        private readonly VSetInfo _defaultSet = new VSetInfo();
        private readonly Dictionary<ItemCodex.ItemTypes, VSetInfo> _alternateSets = new Dictionary<ItemCodex.ItemTypes, VSetInfo>();
        private readonly Dictionary<ItemCodex.ItemTypes, string> _altSetResources = new Dictionary<ItemCodex.ItemTypes, string>();
        private bool _doubleSlot = false;

        private VSetInfo GetSetToUse(ItemCodex.ItemTypes altSet)
        {
            VSetInfo setToUse = _defaultSet;
            if (altSet != ItemCodex.ItemTypes.Null)
            {
                if (!_alternateSets.ContainsKey(altSet)) _alternateSets[altSet] = new VSetInfo();
                setToUse = _alternateSets[altSet];
            }
            return setToUse;
        }

        private ModHatInfoBuilder IsMask(bool doubleSlot)
        {
            _doubleSlot = doubleSlot;
            return this;
        }

        // These methods manipulate the default visual set

        public ModHatInfoBuilder RendersUnderHair(bool up, bool right, bool down, bool left)
        {
            return RendersUnderHair(ItemCodex.ItemTypes.Null, up, right, down, left); ;
        }

        public ModHatInfoBuilder RendersBehindPlayer(bool up, bool right, bool down, bool left)
        {
            return RendersBehindPlayer(ItemCodex.ItemTypes.Null, up, right, down, left); ;
        }

        public ModHatInfoBuilder RenderOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            return RenderOffsets(ItemCodex.ItemTypes.Null, up, right, down, left);
        }

        public ModHatInfoBuilder HairObstruction(bool sides, bool top, bool bottom)
        {
            return HairObstruction(ItemCodex.ItemTypes.Null, sides, top, bottom);
        }

        // These methods manipulate an alternate visual set, i.e. the options used if the given hairdo is equipped

        public ModHatInfoBuilder RendersUnderHair(ItemCodex.ItemTypes hairdoAltSet, bool up, bool right, bool down, bool left)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._sortsUnderHair = new bool[] { up, right, down, left };
            return this;
        }

        public ModHatInfoBuilder RendersBehindPlayer(ItemCodex.ItemTypes hairdoAltSet, bool up, bool right, bool down, bool left)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._sortsBehindChar = new bool[] { up, right, down, left };
            return this;
        }


        public ModHatInfoBuilder RenderOffsets(ItemCodex.ItemTypes hairdoAltSet, Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._renderOffsets = new Vector2[] {
                new Vector2(up.X, up.Y), new Vector2(right.X, right.Y), new Vector2(down.X, down.Y), new Vector2(left.X, left.Y)
            };
            return this;
        }

        public ModHatInfoBuilder HairObstruction(ItemCodex.ItemTypes hairdoAltSet, bool sides, bool top, bool bottom)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._obstructSides = sides;
            setToUse._obstructTop = top;
            setToUse._obstructBottom = bottom;
            return this;
        }

        // This specifies a resource for this hat's alt set.
        // If a hat is located in "Hats/[HatResource]", then GrindScript
        // will take the alt set textures from "Hats/[HatResource]/[AltSetResource]"
        public ModHatInfoBuilder AltSetResource(ItemCodex.ItemTypes hairdoAltSet, string resource)
        {
            _altSetResources[hairdoAltSet] = resource;
            return this;
        }

        // Method hiding used so that this returns the derived class reference

        public new ModHatInfoBuilder Resource(ContentManager manager, string resource)
        {
            return (ModHatInfoBuilder)base.Resource(manager, resource);
        }

        public new ModHatInfoBuilder Stats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            return (ModHatInfoBuilder)base.Stats(HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen);
        }

        public new ModHatInfoBuilder SpecialEffects(params EquipmentInfo.SpecialEffect[] effects)
        {
            return (ModHatInfoBuilder)base.SpecialEffects(effects);
        }

        private void InitializeSet(HatInfo.VisualSet set, VSetInfo desc)
        {
            desc._sortsUnderHair.CopyTo(set.abUnderHair, 0);
            desc._sortsBehindChar.CopyTo(set.abBehindCharacter, 0);
            desc._renderOffsets.CopyTo(set.av2RenderOffsets, 0);
            set.bObstructsSides = desc._obstructSides;
            set.bObstructsTop = desc._obstructTop;
            set.bObstructsBottom = desc._obstructBottom;
        }

        internal override ModEquipInfo Build(ItemCodex.ItemTypes allocatedType)
        {
            HatInfo equipInfo = new HatInfo(allocatedType);
            BasicBuildOnto(equipInfo);

            equipInfo.bDoubleSlot = _doubleSlot;

            InitializeSet(equipInfo.xDefaultSet, _defaultSet);
            foreach (var kvp in _alternateSets)
            {
                HatInfo.VisualSet set = (equipInfo.denxAlternateVisualSets[kvp.Key] = new HatInfo.VisualSet());
                InitializeSet(set, kvp.Value);
            }

            _categoryForBasic = ItemCodex.ItemCategories.Hat;
            return new ModEquipInfo()
            {
                type = allocatedType,
                resourceToUse = _resource,
                managerToUse = _managerToUse,
                vanilla = equipInfo,
                hatAltSetResources = new Dictionary<ItemCodex.ItemTypes, string>(_altSetResources),
                equipCategory = _categoryForBasic
            };
        }
    }

    public class ModWeaponInfoBuilder : ModEquipInfoBuilder
    {
        private WeaponInfo.WeaponCategory _handedness = WeaponInfo.WeaponCategory.OneHanded;
        private bool _magicWeapon = false;

        public ModWeaponInfoBuilder WeaponType(WeaponInfo.WeaponCategory hands, bool magic)
        {
            _handedness = hands;
            _magicWeapon = magic;
            return this;
        }

        // Method hiding used so that this returns the derived class reference

        public new ModWeaponInfoBuilder Resource(ContentManager manager, string resource)
        {
            return (ModWeaponInfoBuilder)base.Resource(manager, resource);
        }

        public new ModWeaponInfoBuilder Stats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            return (ModWeaponInfoBuilder)base.Stats(HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen);
        }

        public new ModWeaponInfoBuilder SpecialEffects(params EquipmentInfo.SpecialEffect[] effects)
        {
            return (ModWeaponInfoBuilder)base.SpecialEffects(effects);
        }

        internal override ModEquipInfo Build(ItemCodex.ItemTypes allocatedType)
        {
            WeaponInfo equipInfo = new WeaponInfo(_resource, allocatedType, _handedness); // TODO add custom palette support
            BasicBuildOnto(equipInfo);

            equipInfo.enWeaponCategory = _handedness;
            equipInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.None;

            if (_handedness == WeaponInfo.WeaponCategory.OneHanded)
            {
                if (_magicWeapon) 
                    equipInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic1H;
                equipInfo.iDamageMultiplier = 90;
            }
            else if (_handedness == WeaponInfo.WeaponCategory.TwoHanded)
            {
                if (_magicWeapon)
                    equipInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic2H;
                equipInfo.iDamageMultiplier = 125;
            }

            switch (_handedness)
            {
                case WeaponInfo.WeaponCategory.OneHanded:
                    _categoryForBasic = ItemCodex.ItemCategories.OneHandedWeapon;
                    break;
                case WeaponInfo.WeaponCategory.TwoHanded:
                    _categoryForBasic = ItemCodex.ItemCategories.TwoHandedWeapon;
                    break;
                default:
                    _categoryForBasic = ItemCodex.ItemCategories.Weapon;
                    break;
            }

            return new ModEquipInfo()
            {
                type = allocatedType,
                resourceToUse = _resource,
                managerToUse = _managerToUse,
                vanilla = equipInfo,
                equipCategory = _categoryForBasic
            };
        }
    }
}
