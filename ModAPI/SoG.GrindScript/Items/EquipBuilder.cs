using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using Stat = SoG.EquipmentInfo.StatEnum;

namespace SoG.Modding
{
    /// <summary>
    /// Used to define a custom item's equipment stats, such as slot used, ATK, DEF, special effects, render options, and other things.
    /// </summary>

    public class EquipBuilder
    {
        // Fields shared by all EquipmentInfos

        static readonly Stat[] _statArray = {
            Stat.HP, Stat.EP, Stat.ATK, Stat.MATK, Stat.DEF, Stat.ASPD, Stat.CSPD, Stat.Crit, Stat.CritDMG, Stat.ShldHP, Stat.EPRegen, Stat.ShldRegen
        };

        string _resource;
        ContentManager _manager;
        readonly Dictionary<Stat, int> _stats = new Dictionary<Stat, int>();
        EquipmentInfo.SpecialEffect[] _effects = new EquipmentInfo.SpecialEffect[0];
        EquipType _type = EquipType.Accessory;

        // Fields specific to FacegearInfo

        bool[] _sortsOverHair = new bool[] { true, true, true, true };
        bool[] _sortsOverHat = new bool[] { true, true, true, true };
        Vector2[] _renderOffsets = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

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

        readonly VSetInfo _defaultSet = new VSetInfo();
        readonly Dictionary<ItemCodex.ItemTypes, VSetInfo> _altSets = new Dictionary<ItemCodex.ItemTypes, VSetInfo>();
        readonly Dictionary<ItemCodex.ItemTypes, string> _altSetResources = new Dictionary<ItemCodex.ItemTypes, string>();
        bool _doubleSlot = false;

        // Fields specific to WeaponInfo

        WeaponInfo.WeaponCategory _hands = WeaponInfo.WeaponCategory.OneHanded;
        bool _isMagic = false;

        // Private helper methods

        private VSetInfo GetOrCreateSet(ItemCodex.ItemTypes altSet)
        {
            if (altSet == ItemCodex.ItemTypes.Null) return _defaultSet;
            return _altSets.TryGetValue(altSet, out VSetInfo exists) ? exists : _altSets[altSet] = new VSetInfo();
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

        // Methods shared by all EquipmentInfos

        public EquipBuilder() { }

        public EquipBuilder(EquipType type)
        {
            EquipmentType(type);
        }

        public EquipBuilder Resource(ContentManager manager, string resource)
        {
            _manager = manager;
            _resource = resource;
            return this;
        }

        public EquipBuilder Stats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            int[] statValueArray = { HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen };

            for (int i = 0; i < statValueArray.Length; i++)
            {
                if (statValueArray[i] == 0) _stats.Remove(_statArray[i]);
                else _stats.Add(_statArray[i], statValueArray[i]);
            }

            return this;
        }

        public EquipBuilder SpecialEffects(params EquipmentInfo.SpecialEffect[] effects)
        {
            _effects = effects;
            return this;
        }

        public EquipBuilder EquipmentType(EquipType type)
        {
            _type = type;
            return this;
        }

        // Methods specific to FacegearInfo

        public EquipBuilder FacegearOverHair(bool up, bool right, bool down, bool left)
        {
            _sortsOverHair = new bool[] { up, right, down, left };
            return this;
        }

        public EquipBuilder FacegearOverHat(bool up, bool right, bool down, bool left)
        {
            _sortsOverHat = new bool[] { up, right, down, left };
            return this;
        }

        public EquipBuilder FacegearOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            _renderOffsets = new Vector2[] {
                new Vector2(up.X, up.Y), new Vector2(right.X, right.Y), new Vector2(down.X, down.Y), new Vector2(left.X, left.Y)
            };
            return this;
        }

        // Methods specific to HatInfo

        public EquipBuilder HatIsMask(bool doubleSlot)
        {
            _doubleSlot = doubleSlot;
            return this;
        }

        public EquipBuilder HatUnderHair(bool up, bool right, bool down, bool left)
        {
            return HatUnderHair(ItemCodex.ItemTypes.Null, up, right, down, left); ;
        }

        public EquipBuilder HatUnderHair(ItemCodex.ItemTypes hairdoAltSet, bool up, bool right, bool down, bool left)
        {
            VSetInfo setToUse = GetOrCreateSet(hairdoAltSet);
            setToUse._sortsUnderHair = new bool[] { up, right, down, left };
            return this;
        }

        public EquipBuilder HatBehindPlayer(bool up, bool right, bool down, bool left)
        {
            return HatBehindPlayer(ItemCodex.ItemTypes.Null, up, right, down, left); ;
        }

        public EquipBuilder HatBehindPlayer(ItemCodex.ItemTypes hairdoAltSet, bool up, bool right, bool down, bool left)
        {
            VSetInfo setToUse = GetOrCreateSet(hairdoAltSet);
            setToUse._sortsBehindChar = new bool[] { up, right, down, left };
            return this;
        }

        public EquipBuilder HatOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            return HatOffsets(ItemCodex.ItemTypes.Null, up, right, down, left);
        }

        public EquipBuilder HatOffsets(ItemCodex.ItemTypes hairdoAltSet, Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            VSetInfo setToUse = GetOrCreateSet(hairdoAltSet);
            setToUse._renderOffsets = new Vector2[] {
                new Vector2(up.X, up.Y), new Vector2(right.X, right.Y), new Vector2(down.X, down.Y), new Vector2(left.X, left.Y)
            };
            return this;
        }

        public EquipBuilder HatHairObstruction(bool sides, bool top, bool bottom)
        {
            return HatHairObstruction(ItemCodex.ItemTypes.Null, sides, top, bottom);
        }

        public EquipBuilder HatHairObstruction(ItemCodex.ItemTypes hairdoAltSet, bool sides, bool top, bool bottom)
        {
            VSetInfo setToUse = GetOrCreateSet(hairdoAltSet);
            setToUse._obstructSides = sides;
            setToUse._obstructTop = top;
            setToUse._obstructBottom = bottom;
            return this;
        }

        public EquipBuilder HatAltSetResource(ItemCodex.ItemTypes hairdoAltSet, string resource)
        {
            _altSetResources[hairdoAltSet] = resource;
            return this;
        }

        // Methods specific to WeaponInfo

        public EquipBuilder WeaponType(WeaponInfo.WeaponCategory hands, bool magic)
        {
            _hands = hands;
            _isMagic = magic;
            return this;
        }

        // Builder Methods

        internal ModEquip Build(ItemCodex.ItemTypes allocatedType)
        {
            EquipmentInfo info;
            EquipType typeToUse = Enum.IsDefined(typeof(EquipType), _type) ? _type : EquipType.Accessory;

            switch (typeToUse)
            {
                case EquipType.Weapon:
                    info = BuildWeapon(allocatedType); break;
                case EquipType.Hat:
                    info = BuildHat(allocatedType); break;
                case EquipType.Facegear:
                    info = BuildFacegear(allocatedType); break;
                default:
                    info = new EquipmentInfo(_resource, allocatedType); break;
            }

            foreach (var kvp in _stats)
                if (kvp.Value != 0) info.deniStatChanges.Add(kvp.Key, kvp.Value);

            foreach (var effect in _effects)
                info.lenSpecialEffects.Add(effect);

            return new ModEquip()
            {
                resourceToUse = _resource,
                managerToUse = _manager,
                equipType = typeToUse,
                vanilla = info,
                hatAltSetResources = _type != EquipType.Hat ? null : new Dictionary<ItemCodex.ItemTypes, string>(_altSetResources)
            };
        }

        private FacegearInfo BuildFacegear(ItemCodex.ItemTypes allocatedType)
        {
            FacegearInfo equipInfo = new FacegearInfo(allocatedType);

            _sortsOverHair.CopyTo(equipInfo.abOverHair, 0);
            _sortsOverHat.CopyTo(equipInfo.abOverHat, 0);
            _renderOffsets.CopyTo(equipInfo.av2RenderOffsets, 0);

            return equipInfo;
        }

        private HatInfo BuildHat(ItemCodex.ItemTypes allocatedType)
        {
            HatInfo equipInfo = new HatInfo(allocatedType) { bDoubleSlot = _doubleSlot };

            InitializeSet(equipInfo.xDefaultSet, _defaultSet);
            foreach (var kvp in _altSets)
                InitializeSet(equipInfo.denxAlternateVisualSets[kvp.Key] = new HatInfo.VisualSet(), kvp.Value);

            return equipInfo;
        }

        private WeaponInfo BuildWeapon(ItemCodex.ItemTypes allocatedType)
        {
            // TODO add custom palette support
            WeaponInfo equipInfo = new WeaponInfo(_resource, allocatedType, _hands)
            {
                enWeaponCategory = _hands,
                enAutoAttackSpell = WeaponInfo.AutoAttackSpell.None
            }; 

            if (_hands == WeaponInfo.WeaponCategory.OneHanded)
            {
                equipInfo.iDamageMultiplier = 90;
                if (_isMagic)
                    equipInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic1H;
            }
            else if (_hands == WeaponInfo.WeaponCategory.TwoHanded)
            {
                equipInfo.iDamageMultiplier = 125;
                if (_isMagic)
                    equipInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic2H;
            }

            return equipInfo;
        }
    }
}
