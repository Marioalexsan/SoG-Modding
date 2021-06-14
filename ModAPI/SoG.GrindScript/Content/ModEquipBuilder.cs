using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EStat = SoG.EquipmentInfo.StatEnum;

namespace SoG.Modding
{
    public enum ModEquipType
    {
        Weapon = ItemCodex.ItemCategories.Weapon,
        Shield = ItemCodex.ItemCategories.Shield,
        Armor = ItemCodex.ItemCategories.Armor,
        Hat = ItemCodex.ItemCategories.Hat,
        Accessory = ItemCodex.ItemCategories.Accessory,
        Shoes = ItemCodex.ItemCategories.Shoes,
        Facegear = ItemCodex.ItemCategories.Facegear
    }

    public class ModEquipBuilder
    {
        // Fields shared by all EquipmentInfos

        private static readonly EStat[] statEnumArray = {
            EStat.HP, EStat.EP, EStat.ATK, EStat.MATK, EStat.DEF, EStat.ASPD, EStat.CSPD, EStat.Crit, EStat.CritDMG, EStat.ShldHP, EStat.EPRegen, EStat.ShldRegen
        };

        private string _resource;
        private ContentManager _managerToUse;
        private Dictionary<EStat, int> _stats = new Dictionary<EStat, int>();
        private EquipmentInfo.SpecialEffect[] _specialEffects = new EquipmentInfo.SpecialEffect[0];
        private ModEquipType _type = ModEquipType.Accessory;

        // Fields specific to FacegearInfo

        private bool[] _sortsOverHair = new bool[] { true, true, true, true };
        private bool[] _sortsOverHat = new bool[] { true, true, true, true };
        private Vector2[] _renderOffsets = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

        // Fields specific to HatInfo

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

        // Fields specific to WeaponInfo

        private WeaponInfo.WeaponCategory _handedness = WeaponInfo.WeaponCategory.OneHanded;
        private bool _magicWeapon = false;

        // Methods shared by all EquipmentInfos

        public ModEquipBuilder() { }

        public ModEquipBuilder(ModEquipType type)
        {
            EquipmentType(type);
        }

        public ModEquipBuilder Resource(ContentManager manager, string resource)
        {
            _managerToUse = manager;
            _resource = resource;
            return this;
        }

        public ModEquipBuilder Stats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            int[] statValueArray = { HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen };

            for (int i = 0; i < statValueArray.Length; i++)
            {
                if (statValueArray[i] == 0) _stats.Remove(statEnumArray[i]);
                else _stats.Add(statEnumArray[i], statValueArray[i]);
            }

            return this;
        }

        public ModEquipBuilder SpecialEffects(params EquipmentInfo.SpecialEffect[] effects)
        {
            _specialEffects = effects;
            return this;
        }

        public ModEquipBuilder EquipmentType(ModEquipType type)
        {
            _type = type;
            return this;
        }

        // Methods specific to FacegearInfo

        public ModEquipBuilder FacegearOverHair(bool up, bool right, bool down, bool left)
        {
            _sortsOverHair = new bool[] { up, right, down, left };
            return this;
        }

        public ModEquipBuilder FacegearOverHat(bool up, bool right, bool down, bool left)
        {
            _sortsOverHat = new bool[] { up, right, down, left };
            return this;
        }

        public ModEquipBuilder FacegearOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            _renderOffsets = new Vector2[] {
                new Vector2(up.X, up.Y), new Vector2(right.X, right.Y), new Vector2(down.X, down.Y), new Vector2(left.X, left.Y)
            };
            return this;
        }

        // Methods specific to HatInfo

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

        public ModEquipBuilder HatIsMask(bool doubleSlot)
        {
            _doubleSlot = doubleSlot;
            return this;
        }

        public ModEquipBuilder HatUnderHair(bool up, bool right, bool down, bool left)
        {
            return HatUnderHair(ItemCodex.ItemTypes.Null, up, right, down, left); ;
        }

        public ModEquipBuilder HatUnderHair(ItemCodex.ItemTypes hairdoAltSet, bool up, bool right, bool down, bool left)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._sortsUnderHair = new bool[] { up, right, down, left };
            return this;
        }

        public ModEquipBuilder HatBehindPlayer(bool up, bool right, bool down, bool left)
        {
            return HatBehindPlayer(ItemCodex.ItemTypes.Null, up, right, down, left); ;
        }

        public ModEquipBuilder HatBehindPlayer(ItemCodex.ItemTypes hairdoAltSet, bool up, bool right, bool down, bool left)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._sortsBehindChar = new bool[] { up, right, down, left };
            return this;
        }

        public ModEquipBuilder HatOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            return HatOffsets(ItemCodex.ItemTypes.Null, up, right, down, left);
        }

        public ModEquipBuilder HatOffsets(ItemCodex.ItemTypes hairdoAltSet, Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._renderOffsets = new Vector2[] {
                new Vector2(up.X, up.Y), new Vector2(right.X, right.Y), new Vector2(down.X, down.Y), new Vector2(left.X, left.Y)
            };
            return this;
        }

        public ModEquipBuilder HatHairObstruction(bool sides, bool top, bool bottom)
        {
            return HatHairObstruction(ItemCodex.ItemTypes.Null, sides, top, bottom);
        }

        public ModEquipBuilder HatHairObstruction(ItemCodex.ItemTypes hairdoAltSet, bool sides, bool top, bool bottom)
        {
            VSetInfo setToUse = GetSetToUse(hairdoAltSet);
            setToUse._obstructSides = sides;
            setToUse._obstructTop = top;
            setToUse._obstructBottom = bottom;
            return this;
        }

        public ModEquipBuilder HatAltSetResource(ItemCodex.ItemTypes hairdoAltSet, string resource)
        {
            _altSetResources[hairdoAltSet] = resource;
            return this;
        }

        // Methods specific to WeaponInfo

        public ModEquipBuilder WeaponType(WeaponInfo.WeaponCategory hands, bool magic)
        {
            _handedness = hands;
            _magicWeapon = magic;
            return this;
        }

        // Builder Methods

        protected void BuildOntoEquipmentInfo(EquipmentInfo info)
        {
            foreach (var kvp in _stats)
                if (kvp.Value != 0) info.deniStatChanges.Add(kvp.Key, kvp.Value);

            foreach (var effect in _specialEffects)
                info.lenSpecialEffects.Add(effect);
        }

        internal ModEquip Build(ItemCodex.ItemTypes allocatedType)
        {
            EquipmentInfo builtInfo;
            switch((ItemCodex.ItemCategories)_type)
            {
                case ItemCodex.ItemCategories.Weapon:
                    builtInfo = BuildWeapon(allocatedType);
                    break;
                case ItemCodex.ItemCategories.Hat:
                    builtInfo = BuildHat(allocatedType);
                    break;
                case ItemCodex.ItemCategories.Facegear:
                    builtInfo = BuildFacegear(allocatedType);
                    break;
                default:
                    builtInfo = BuildEquipment(allocatedType);
                    break;
            }
            return new ModEquip()
            {
                type = allocatedType,
                resourceToUse = _resource,
                managerToUse = _managerToUse,
                equipType = _type,
                vanilla = builtInfo,
                hatAltSetResources = _type != ModEquipType.Hat ? null : new Dictionary<ItemCodex.ItemTypes, string>(_altSetResources)
            };
        }

        internal EquipmentInfo BuildEquipment(ItemCodex.ItemTypes allocatedType)
        {
            EquipmentInfo equipInfo = new EquipmentInfo(_resource, allocatedType);
            BuildOntoEquipmentInfo(equipInfo);
            return equipInfo;
        }

        internal FacegearInfo BuildFacegear(ItemCodex.ItemTypes allocatedType)
        {
            FacegearInfo equipInfo = new FacegearInfo(allocatedType);
            BuildOntoEquipmentInfo(equipInfo);

            _sortsOverHair.CopyTo(equipInfo.abOverHair, 0);
            _sortsOverHat.CopyTo(equipInfo.abOverHat, 0);
            _renderOffsets.CopyTo(equipInfo.av2RenderOffsets, 0);

            return equipInfo;
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

        internal HatInfo BuildHat(ItemCodex.ItemTypes allocatedType)
        {
            HatInfo equipInfo = new HatInfo(allocatedType);
            BuildOntoEquipmentInfo(equipInfo);

            equipInfo.bDoubleSlot = _doubleSlot;

            InitializeSet(equipInfo.xDefaultSet, _defaultSet);
            foreach (var kvp in _alternateSets)
            {
                HatInfo.VisualSet set = (equipInfo.denxAlternateVisualSets[kvp.Key] = new HatInfo.VisualSet());
                InitializeSet(set, kvp.Value);
            }

            return equipInfo;
        }

        internal WeaponInfo BuildWeapon(ItemCodex.ItemTypes allocatedType)
        {
            WeaponInfo equipInfo = new WeaponInfo(_resource, allocatedType, _handedness); // TODO add custom palette support
            BuildOntoEquipmentInfo(equipInfo);

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

            return equipInfo;
        }
    }
}
