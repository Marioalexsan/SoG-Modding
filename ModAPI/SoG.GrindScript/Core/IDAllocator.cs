using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Allocates IDs and other things.
    /// </summary>

    internal static class IDAllocator
    {
		public const ItemCodex.ItemTypes ItemTypes_ShuffleStart = (ItemCodex.ItemTypes)1500000;
		public const RogueLikeMode.Perks RoguelikePerk_ShuffleStart = (RogueLikeMode.Perks)9000;
		public const RogueLikeMode.TreatsCurses TreatsCurses_ShuffleStart = (RogueLikeMode.TreatsCurses)9000;

		public const ItemCodex.ItemTypes ItemTypesStart = (ItemCodex.ItemTypes)700000;
		public static ItemCodex.ItemTypes ItemTypesEnd { get; private set; } = ItemTypesStart;

		public const EquipmentInfo.SpecialEffect EquipEffectStart = (EquipmentInfo.SpecialEffect)700;
		public static EquipmentInfo.SpecialEffect EquipEffectEnd { get; private set; } = EquipEffectStart;

		public const int AudioIDStart = 0;
		public static int AudioIDNext { get; private set; } = AudioIDStart;

		public const Level.ZoneEnum ZoneEnumStart = (Level.ZoneEnum)5600;
		public static Level.ZoneEnum ZoneEnumEnd { get; private set; } = ZoneEnumStart;

		public const Level.WorldRegion WorldRegionStart = (Level.WorldRegion)650;
		public static Level.WorldRegion WorldRegionEnd { get; private set; } = WorldRegionStart;

		public const RogueLikeMode.Perks RoguelikePerkStart = (RogueLikeMode.Perks)3500;
		public static RogueLikeMode.Perks RoguelikePerkEnd { get; private set; } = RoguelikePerkStart;

		public const RogueLikeMode.TreatsCurses TreatsCursesStart = (RogueLikeMode.TreatsCurses)3500;
		public static RogueLikeMode.TreatsCurses TreatsCursesEnd { get; private set; } = TreatsCursesStart;

		internal static ItemCodex.ItemTypes NewItemType() => ItemTypesEnd++;

		internal static EquipmentInfo.SpecialEffect NewEquipEffect() => EquipEffectEnd++;

		internal static int NewAudioEntry() => AudioIDNext++;

		internal static Level.ZoneEnum NewZoneEnum() => ZoneEnumEnd++;

		internal static Level.WorldRegion NewWorldRegion() => WorldRegionEnd++;

		internal static RogueLikeMode.Perks NewRoguelikePerk() => RoguelikePerkEnd++;

		internal static RogueLikeMode.TreatsCurses NewTreatOrCurse() => TreatsCursesEnd++;
	}
}
