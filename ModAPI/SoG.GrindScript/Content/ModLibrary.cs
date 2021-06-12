using System.Collections.Generic;

namespace SoG.Modding
{
    public class ModLibrary
	{
		internal const ItemCodex.ItemTypes ItemTypesStart = (ItemCodex.ItemTypes)400000;
		internal const EquipmentInfo.SpecialEffect SpecialEffectsStart = (EquipmentInfo.SpecialEffect)200;

		//internal readonly Dictionary<ItemCodex.ItemTypes, ModItemData> ItemDetails = new Dictionary<ItemCodex.ItemTypes, ModItemData>();

		// "Non-Owning" Helper dictionaries, references, etc.

		internal readonly Dictionary<string, ItemCodex.ItemTypes> RegisteredItems = new Dictionary<string, ItemCodex.ItemTypes>(); // Correlates one string to one item ID - utility for modders
		internal readonly Dictionary<string, EquipmentInfo.SpecialEffect> RegisteredEquipmentEffects = new Dictionary<string, EquipmentInfo.SpecialEffect>(); // Correlates one string to one special effect ID - utility for modders

		// Helper values and properties for enum real estate

		//internal ItemCodex.ItemTypes ItemTypesNext => ItemTypesStart + ItemDetails.Count;

		internal ushort SpecialEffectsCount = 0;
		internal EquipmentInfo.SpecialEffect SpecialEffectsNext => SpecialEffectsStart + SpecialEffectsCount;

		// Utility Functions

		public void AddItemAlias(string name, ItemCodex.ItemTypes enType)
		{
			RegisteredItems.Add(name, enType);
		}

		public ItemCodex.ItemTypes ItemAliasValue(string name)
		{
			ItemCodex.ItemTypes enType;
			if (!RegisteredItems.TryGetValue(name, out enType))
			{
				enType = (ItemCodex.ItemTypes)(-1);
			}
			return enType;
		}

		public void AddItemEffectAlias(string name, EquipmentInfo.SpecialEffect enType)
		{
			RegisteredEquipmentEffects.Add(name, enType);
		}

		public EquipmentInfo.SpecialEffect ItemEffectAliasValue(string name)
		{
			EquipmentInfo.SpecialEffect enType;
			if (!RegisteredEquipmentEffects.TryGetValue(name, out enType))
			{
				enType = (EquipmentInfo.SpecialEffect)(ushort.MaxValue);
			}
			return enType;
		}
	}
}