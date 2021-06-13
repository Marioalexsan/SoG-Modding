using System.Collections.Generic;

namespace SoG.Modding
{
	/// <summary>
	/// Holds modded game content
	/// </summary>
    internal class ModLibrary
	{
		// Constants

		public const ItemCodex.ItemTypes ItemTypesStart = (ItemCodex.ItemTypes)400000;
		public const EquipmentInfo.SpecialEffect EquipEffectStart = (EquipmentInfo.SpecialEffect)200;

		// Enum real estate - should be static due to acting as allocation for SoG

		internal static ItemCodex.ItemTypes ItemTypesNext = ItemTypesStart;
		internal static EquipmentInfo.SpecialEffect EquipEffectNext = EquipEffectStart;

		// Modded content - holds a specific mod's added content (except for GrindScript, where the library holds all content added)

		public readonly Dictionary<ItemCodex.ItemTypes, ModItem> ModItems = new Dictionary<ItemCodex.ItemTypes, ModItem>();

		// Dictionaries holding aliases - theoretically useful for human-readable and run time independent "IDs"

		public readonly Dictionary<string, ItemCodex.ItemTypes> ItemAliases = new Dictionary<string, ItemCodex.ItemTypes>();
		public readonly Dictionary<string, EquipmentInfo.SpecialEffect> EquipEffectAliases = new Dictionary<string, EquipmentInfo.SpecialEffect>();

		// Utility Functions

		internal static ItemCodex.ItemTypes AllocateItemType()
        {
			return ItemTypesNext++;
		}

		internal static EquipmentInfo.SpecialEffect AllocateItemEffectAlias()
        {
			return EquipEffectNext++;
        }
	}
}