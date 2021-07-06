using System.Collections.Generic;
using System;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Allocates IDs and other things.
    /// </summary>
    internal class IDAllocator
    {
		/// <summary>
		/// Allocates int IDs sequentially
		/// </summary>
		public class IntAlloc
        {
			public int Start { get; private set; }

			public int End { get; private set; }

			public int Max { get; private set; }

			public int Allocate() => End < Max ? End++ : End;

			public IntAlloc(int start, int count)
            {
				Start = start;
				End = start;
				Max = start + count;
            }
		}

		/// <summary>
		/// Allocates enum IDs sequentially
		/// </summary>
		public class EnumAlloc<IDType> : IntAlloc where IDType : Enum
        {
			public new IDType Start { get => (IDType)Enum.ToObject(typeof(IDType), base.Start); }

			public new IDType End { get => (IDType)Enum.ToObject(typeof(IDType), base.End); }

			public new IDType Max { get => (IDType)Enum.ToObject(typeof(IDType), base.Max); }

			public new IDType Allocate() => (IDType)Enum.ToObject(typeof(IDType), base.Allocate());

			public EnumAlloc(int start, int count)
				: base(start, count) { }
		}

		public EnumAlloc<ItemCodex.ItemTypes> ItemID = new EnumAlloc<ItemCodex.ItemTypes>(700000, 100000);

		public EnumAlloc<EquipmentInfo.SpecialEffect> EquipEffectID = new EnumAlloc<EquipmentInfo.SpecialEffect>(700, 5000);

		public IntAlloc ModIndexID = new IntAlloc(0, 65536);

		public EnumAlloc<Level.ZoneEnum> LevelID = new EnumAlloc<Level.ZoneEnum>(5600, 10000);

		public EnumAlloc<Level.WorldRegion> WorldRegionID = new EnumAlloc<Level.WorldRegion>(650, 5000);

		public EnumAlloc<RogueLikeMode.Perks> PerkID = new EnumAlloc<RogueLikeMode.Perks>(3500, 1000);

		public EnumAlloc<RogueLikeMode.TreatsCurses> TreatCurseID = new EnumAlloc<RogueLikeMode.TreatsCurses>(3500, 1000);
	}
}
