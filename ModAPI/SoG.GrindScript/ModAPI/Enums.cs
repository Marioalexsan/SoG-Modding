namespace SoG.Modding
{
    /// <summary>
    /// Defines various misc text types for UI <para/>
    /// </summary>

    public enum MiscTextTypes
    {
        Default,
        GenericItemDescription,
        GenericItemName,
        GenericSpellName,
        GenericSpellFlavor,
        GenericSpellMoreInfo
    }

    /// <summary>
    /// Defines equipment types based on ItemCodex.ItemCategories.
    /// </summary>

    public enum EquipType
    {
        Weapon = ItemCodex.ItemCategories.Weapon,
        Shield = ItemCodex.ItemCategories.Shield,
        Armor = ItemCodex.ItemCategories.Armor,
        Hat = ItemCodex.ItemCategories.Hat,
        Accessory = ItemCodex.ItemCategories.Accessory,
        Shoes = ItemCodex.ItemCategories.Shoes,
        Facegear = ItemCodex.ItemCategories.Facegear
    }
}


