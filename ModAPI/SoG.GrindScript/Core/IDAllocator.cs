using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Allocates IDs and other things.
    /// </summary>

    internal static class IDAllocator
    {
		public const ItemCodex.ItemTypes ItemTypesStart = (ItemCodex.ItemTypes)700000;
		public static ItemCodex.ItemTypes ItemTypesNext { get; private set; } = ItemTypesStart;

		public const ItemCodex.ItemTypes ItemTypes_ShuffleStart = (ItemCodex.ItemTypes)1500000;

		public const EquipmentInfo.SpecialEffect EquipEffectStart = (EquipmentInfo.SpecialEffect)700;
		public static EquipmentInfo.SpecialEffect EquipEffectNext { get; private set; } = EquipEffectStart;

		public const int AudioIDStart = 0;
		public static int AudioIDNext { get; private set; } = AudioIDStart;

		public const Level.ZoneEnum ZoneEnumStart = (Level.ZoneEnum)650;
		public static Level.ZoneEnum ZoneEnumNext { get; private set; } = ZoneEnumStart;

		public const Level.WorldRegion WorldRegionStart = (Level.WorldRegion)650;
		public static Level.WorldRegion WorldRegionNext { get; private set; } = WorldRegionStart;

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

		internal static Level.ZoneEnum AllocateZoneEnum()
		{
			return ZoneEnumNext++;
		}

		internal static Level.WorldRegion AllocateWorldRegion()
		{
			return WorldRegionNext++;
		}
	}
}
