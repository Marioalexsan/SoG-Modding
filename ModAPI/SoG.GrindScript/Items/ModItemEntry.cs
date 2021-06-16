using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SoG.Modding
{
    // Represents a modded item in the ModLibrary
    internal class ModItemEntry
    {
        public BaseScript owner;

        public ItemCodex.ItemTypes type;

        public ModItem itemInfo;

        public ModEquip equipInfo;
    }

    // Stores additional data for a modded item description
    internal class ModItem
    {
        public ItemDescription vanilla;

        public string shadowToUse = "";

        public string resourceToUse = "";

        public ContentManager managerToUse;
    }

    // Stores additional data for a modded equipment
    internal class ModEquip
    {
        public EquipmentInfo vanilla;

        public string resourceToUse = "";

        public ContentManager managerToUse;

        public Dictionary<ItemCodex.ItemTypes, string> hatAltSetResources;

        public ModEquipType equipType;
    }
}
