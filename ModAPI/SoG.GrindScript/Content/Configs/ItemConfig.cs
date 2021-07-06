using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using Stat = SoG.EquipmentInfo.StatEnum;
using SoG.Modding.Core;

namespace SoG.Modding.Content.Configs
{
    /// <summary>
    /// Used to define a custom item's basic properties, such as its name, value, item categories, and other things.
    /// </summary>

    public class ItemConfig
    {
        public class VSetInfo
        {
            /// <summary> For hats, sets if it renders under hair (facing up, right, down, and left). </summary>
            public bool[] HatUnderHair { get; } = new bool[] { false, false, false, false };

            /// <summary> For hats, sets if it renders behind the player (facing up, right, down, and left). </summary>
            public bool[] HatBehindPlayer { get; } = new bool[] { false, false, false, false };

            /// <summary> For hats, sets the render offsets (facing up, right, down, and left). </summary>
            public Vector2[] HatOffsets { get; } = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

            /// <summary> For hats, sets whenever the hat hides hair sides. </summary>
            public bool ObstructHairSides { get; set; } = true;

            /// <summary> For hats, sets whenever the hat hides hair top. </summary>
            public bool ObstructHairTop { get; set; } = true;

            /// <summary> For hats, sets whenever the hat hides hair bottom. </summary>
            public bool ObstructHairBottom { get; set; } = false;

            /// <summary> The subfolder where the textures are for this style, relative to the hat textures folder. </summary>
            public string Resource { get; set; } = "";
        }

        /// <summary>
        /// Creates a new ItemConfig. UniqueID must be unique between all other items in the same mod.
        /// </summary>

        public ItemConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        // ItemDescription

        /// <summary> An identifier that must be unique between all other items in the same mod. </summary>
        public string ModID { get; set; } = "";

        public string Name { get; set; } = "A Mod Item";

        public string Description { get; set; } = "Description pending!";

        /// <summary> The path to the drop appearance texture of an item, relative to "Content/", and without the ".xnb" extension. </summary>
        public string IconPath { get; set; } = "";

        /// <summary> The path to the drop shadow texture of an item, relative to "Content/", and without the ".xnb" extension. </summary>
        public string ShadowPath { get; set; } = "";

        /// <summary> Gold value of an item. For fresh players, this is identical to the buy price, and twice the sell price. </summary>
        public int Value { get; set; } = 1;

        /// <summary> Overrides the health cost of this item when bought from the Shadier Merchant. If set to 0, the game calculates it from the gold cost. </summary>
        public int BloodValue { get; set; } = 0;

        /// <summary> Gold value modifier for items when playing in Arcade Mode. </summary>
        public float ArcadeValueModifier { get; set; } = 1f;

        /// <summary> Used for sorting. Items with higher sorting values appear first when sorting by "Best". </summary>
        public ushort SortingValue { get; set; } = 1;

        /// <summary> Determines if item drops of this type have a special visual effect. Valid values are 1 (none), 2 (silver ring) and 3 (gold ring) </summary>
        public byte Fancyness { get; set; } = 1;

        /// <summary> The ContentManager to use for this item's textures. By default, Game1.Content is used. </summary>
        public ContentManager Manager { get; set; } = APIGlobals.Game.Content;

        /// <summary> Item Categories to assign to this item. GrindScript automatically assigns certain categories for equipments. </summary>
        public HashSet<ItemCodex.ItemCategories> Categories { get; } = new HashSet<ItemCodex.ItemCategories>();



        /// <summary> 
        /// Sets the categories for this item. 
        /// </summary>

        public ItemConfig SetCategories(params ItemCodex.ItemCategories[] categories)
        {
            Categories.Clear();
            Categories.UnionWith(categories);
            return this;
        }

        // EquipmentInfo - shared

        /// <summary> The path to the folder containing the textures needed for the equipment. </summary>
        public string EquipResourcePath { get; set; } = "";

        /// <summary> The stats that this equipment provides. The stat property shorthands modify this dictionary. </summary>
        public Dictionary<Stat, int> Stats { get; } = new Dictionary<Stat, int>();

        public int HP { get => Stats[Stat.HP]; set => Stats[Stat.HP] = value; }

        public int EP { get => Stats[Stat.EP]; set => Stats[Stat.EP] = value; }

        public int ATK { get => Stats[Stat.ATK]; set => Stats[Stat.ATK] = value; }

        public int MATK { get => Stats[Stat.MATK]; set => Stats[Stat.MATK] = value; }

        public int DEF { get => Stats[Stat.DEF]; set => Stats[Stat.DEF] = value; }

        public int ASPD { get => Stats[Stat.ASPD]; set => Stats[Stat.ASPD] = value; }

        public int CSPD { get => Stats[Stat.CSPD]; set => Stats[Stat.CSPD] = value; }

        public int Crit { get => Stats[Stat.Crit]; set => Stats[Stat.Crit] = value; }

        public int CritDMG { get => Stats[Stat.CritDMG]; set => Stats[Stat.CritDMG] = value; }

        public int ShldHP { get => Stats[Stat.ShldHP]; set => Stats[Stat.ShldHP] = value; }

        public int EPRegen { get => Stats[Stat.EPRegen]; set => Stats[Stat.EPRegen] = value; }

        public int ShldRegen { get => Stats[Stat.ShldRegen]; set => Stats[Stat.ShldRegen] = value; }

        /// <summary> 
        /// The equipment type of this item.
        /// Certain config settings are ignored depending on the EquipType (for example, hat appearance settings for weapons). <para/>
        /// A value of EquipType.None will create a non-equipment item.
        /// </summary>
        public EquipmentType EquipType { get; set; } = EquipmentType.None;

        /// <summary> The Special Effects that this equipment has. Multiple special effects can be added. </summary>
        public HashSet<EquipmentInfo.SpecialEffect> Effects { get; } = new HashSet<EquipmentInfo.SpecialEffect>();

        // FacegearInfo

        /// <summary> For facegear, sets if it renders over hair (facing up, right, down, and left). </summary>
        public bool[] FacegearOverHair { get; } = new bool[] { true, true, true, true };

        /// <summary> For facegear, sets if it renders over the hat (facing up, right, down, and left). </summary>
        public bool[] FacegearOverHat { get; } = new bool[] { true, true, true, true };

        /// <summary> For facegear, sets the render offsets (facing up, right, down, and left). </summary>
        public Vector2[] FacegearOffsets { get; } = new Vector2[] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };

        // HatInfo

        /// <summary> For hats, sets the default visual appearance. </summary>
        public VSetInfo DefaultSet { get; } = new VSetInfo();

        /// <summary> For hats, sets the visual appearance when the player has a certain hair style. </summary>
        public Dictionary<ItemCodex.ItemTypes, VSetInfo> AltSets { get; } = new Dictionary<ItemCodex.ItemTypes, VSetInfo>();

        /// <summary> For hats, sets whenever it occupies both the hat and the facegear slot (i.e. it is not possible to equip a facegear alongside this hat). </summary>
        public bool HatDoubleSlot { get; set; } = false;

        // WeaponInfo

        /// <summary> For weapons, sets the melee class (One-Handed or Two-Handed). </summary>
        public WeaponInfo.WeaponCategory WeaponType { get; set; } = WeaponInfo.WeaponCategory.OneHanded;

        /// <summary> For weapons, causes it to act as a magic weapon (i.e. basic attacks shoot projectiles and draw their damage from both ATK and MATK). </summary>
        public bool MagicWeapon { get; set; } = false;

        // Helper Methods

        private VSetInfo GetOrCreateSet(ItemCodex.ItemTypes altSet)
        {
            if (altSet == ItemCodex.ItemTypes.Null)
                return DefaultSet;

            return AltSets.TryGetValue(altSet, out VSetInfo exists) ? exists : AltSets[altSet] = new VSetInfo();
        }

        // Methods shared by all EquipmentInfos

        // Methods specific to FacegearInfo

        public ItemConfig SetFacegearOverHair(bool up, bool right, bool down, bool left)
        {
            Array.Copy(new bool[] { up, right, down, left }, FacegearOverHair, 4);
            return this;
        }

        public ItemConfig SetFacegearOverHat(bool up, bool right, bool down, bool left)
        {
            Array.Copy(new bool[] { up, right, down, left }, FacegearOverHat, 4);
            return this;
        }

        public ItemConfig SetFacegearOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            Array.Copy(new Vector2[] { up, right, down, left }, FacegearOffsets, 4);
            return this;
        }

        // Methods specific to HatInfo

        public ItemConfig SetHatUnderHair(bool up, bool right, bool down, bool left, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            Array.Copy(new bool[] { up, right, down, left }, setToUse.HatUnderHair, 4);
            return this;
        }

        public ItemConfig SetHatBehindPlayer(bool up, bool right, bool down, bool left, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            Array.Copy(new bool[] { up, right, down, left }, setToUse.HatBehindPlayer, 4);
            return this;
        }

        public ItemConfig SetHatOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            Array.Copy(new Vector2[] { up, right, down, left }, setToUse.HatOffsets, 4);
            return this;
        }

        public ItemConfig SetHatHairObstruction(bool sides, bool top, bool bottom, ItemCodex.ItemTypes targetHairStyle = ItemCodex.ItemTypes.Null)
        {
            VSetInfo setToUse = GetOrCreateSet(targetHairStyle);
            setToUse.ObstructHairSides = sides;
            setToUse.ObstructHairTop = top;
            setToUse.ObstructHairBottom = bottom;
            return this;
        }

        public ItemConfig SetHatAltSetResource(ItemCodex.ItemTypes hairdoAltSet, string resource)
        {
            VSetInfo setToUse = GetOrCreateSet(hairdoAltSet);
            if (setToUse != DefaultSet)
                setToUse.Resource = resource;
            return this;
        }
    }
}
