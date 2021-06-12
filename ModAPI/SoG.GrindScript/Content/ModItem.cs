using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SoG.Modding
{
    public static partial class SoGExtension
    {
        public static bool IsSoGItem(this ItemCodex.ItemTypes enType)
        {
            return Enum.IsDefined(typeof(ItemCodex.ItemTypes), enType);
        }

        public static bool IsModItem(this ItemCodex.ItemTypes enType)
        {
            return false;
            //return enType >= ModLibrary.ItemTypesStart && enType < GrindScript.ModLib.ItemTypesNext;
        }
    }

    public class ModItem
    {
        public BaseScript owner;

        public ItemDescription info;

        public EquipmentInfo equipInfo;

        public Texture2D shadowToUse;

        public ContentManager managerToUse;
    }

    public class ModItemBuilder
    {
        private string _name;
        private string _description;
        private string _displayTexPath;
        private string _shadowTexPath;

        private int _value;
        private int _bloodValue;
        private float _arcadeValueMod;

        private ContentManager _managerToUse;
        private ItemCodex.ItemCategories[] _categories;

        public ModItemBuilder() { }

        public ModItemBuilder(string name, string description)
        {
            SetTexts(name, description);
        }

        public ModItemBuilder SetTexts(string name, string description)
        {
            _name = name;
            _description = description;
            return this;
        }

        public ModItemBuilder SetTextures(ContentManager manager, string displayTexPath, string shadowTexPath)
        {
            _managerToUse = manager;
            _displayTexPath = displayTexPath;
            _shadowTexPath = shadowTexPath;
            return this;
        }

        public ModItemBuilder SetCategories(params ItemCodex.ItemCategories[] categories)
        {
            _categories = categories;
            return this;
        }

        public ModItemBuilder SetValue(int value, float arcadeModifier = 1f, int bloodCost = 0)
        {
            _value = value;
            _arcadeValueMod = arcadeModifier;
            _bloodValue = bloodCost;
            return this;
        }

        internal ModItem Build(BaseScript owner)
        {
            return null;
        }
    }
}
