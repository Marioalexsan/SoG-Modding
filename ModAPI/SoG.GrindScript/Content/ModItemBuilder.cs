using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public class ModItemBuilder
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
                enType = allocatedType
            };

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
