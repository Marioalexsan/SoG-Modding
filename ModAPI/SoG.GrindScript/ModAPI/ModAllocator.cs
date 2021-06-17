namespace SoG.Modding
{
    /// <summary>
    /// Allocates IDs and other things.
    /// </summary>

    internal static class ModAllocator
    {
		public const ItemCodex.ItemTypes ItemTypesStart = (ItemCodex.ItemTypes)400000;
		public static ItemCodex.ItemTypes ItemTypesNext { get; private set; } = ItemTypesStart;

		public const EquipmentInfo.SpecialEffect EquipEffectStart = (EquipmentInfo.SpecialEffect)200;
		public static EquipmentInfo.SpecialEffect EquipEffectNext { get; private set; } = EquipEffectStart;

		public const int AudioIDStart = 0;
		public static int AudioIDNext { get; private set; } = AudioIDStart;

		internal static ItemCodex.ItemTypes AllocateItemType()
		{
			return ItemTypesNext++; // Naive implementation
		}

		internal static EquipmentInfo.SpecialEffect AlocateEquipEffect()
		{
			return EquipEffectNext++;
		}

		internal static int AllocateAudioEntry()
		{
			return AudioIDNext++;
		}
	}
}
