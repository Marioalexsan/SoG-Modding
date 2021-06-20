using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Represents a modded item in the ModLibrary.
    /// </summary>

    internal class ModItemEntry
    {
        public BaseScript owner;

        public ItemCodex.ItemTypes type;

        public ModItem itemInfo;

        public ModEquip equipInfo;
    }

    /// <summary>
    /// Stores data for a modded item description.
    /// </summary>

    internal class ModItem
    {
        public ItemDescription vanilla;

        public string shadowToUse = "";

        public string resourceToUse = "";

        public ContentManager managerToUse;
    }

    /// <summary>
    /// Stores data for a modded equipment info.
    /// </summary>

    internal class ModEquip
    {
        public EquipmentInfo vanilla;

        public string resourceToUse = "";

        public ContentManager managerToUse;

        public Dictionary<ItemCodex.ItemTypes, string> hatAltSetResources;

        public EquipType equipType;
    }
}
