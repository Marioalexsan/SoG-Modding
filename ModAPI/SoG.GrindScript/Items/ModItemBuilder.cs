using Microsoft.Xna.Framework.Content;
using System;

namespace SoG.Modding
{
    /// <summary>
    /// Used to define a custom item's basic properties, such as its name, value, item categories, and other things.
    /// </summary>

    public class ModItemBuilder
    {
        private string _name = "Unknown Mod Item";
        private string _description = "It's all mysterious and stuff!";
        private string _displayTexPath;
        private string _shadowTexPath;

        private int _value = 1;
        private int _bloodValue = 0;
        private float _arcadeValueMod = 1f;

        private ushort _levelForBestSort = 1;
        private byte _fancyness = 1;

        private ContentManager _managerToUse;
        private ItemCodex.ItemCategories[] _categories = new ItemCodex.ItemCategories[0];

        public ModItemBuilder() { }

        public ModItemBuilder(string name, string description)
        {
            Texts(name, description);
        }

        public ModItemBuilder Texts(string name, string description)
        {
            _name = name;
            _description = description;
            return this;
        }

        public ModItemBuilder Resources(ContentManager manager, string displayTexPath)
        {
            return Resources(manager, displayTexPath, _shadowTexPath);
        }

        public ModItemBuilder Resources(ContentManager manager, string displayTexPath, string shadowTexPath)
        {
            _managerToUse = manager;
            _displayTexPath = displayTexPath;
            _shadowTexPath = shadowTexPath;
            return this;
        }

        public ModItemBuilder Categories(params ItemCodex.ItemCategories[] categories)
        {
            _categories = categories;
            return this;
        }

        public ModItemBuilder Value(int value, float arcadeModifier = 1f, int bloodCost = 0)
        {
            _value = value;
            _arcadeValueMod = arcadeModifier;
            _bloodValue = bloodCost;
            return this;
        }

        public ModItemBuilder Level(ushort levelForBestSort)
        {
            _levelForBestSort = levelForBestSort;
            return this;
        }

        public ModItemBuilder Fancyness(byte fancyness)
        {
            _fancyness = fancyness;
            return this;
        }

        internal ModItem Build(ItemCodex.ItemTypes allocatedType)
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
                fArcadeModeCostModifier = _arcadeValueMod,
                enType = allocatedType
            };

            foreach (var cat in _categories)
                itemInfo.lenCategory.Add(cat);

            return new ModItem()
            {
                vanilla = itemInfo,
                shadowToUse = _shadowTexPath,
                managerToUse = _managerToUse,
                resourceToUse = _displayTexPath
            };
        }
    }
}
