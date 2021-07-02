using System.Collections.Generic;
using System.Linq;

namespace SoG.Modding.Core
{
	// I wanted to make something fancy that auto - updates when content is added
	// but just maintaining both libraries is probably better

    /// <summary>
    /// Holds all mod-relevant content.
    /// </summary>

	internal class PersistentLibrary
	{
		public Dictionary<ItemCodex.ItemTypes, ModItemEntry> Items { get; } = new Dictionary<ItemCodex.ItemTypes, ModItemEntry>();

		public Dictionary<RogueLikeMode.TreatsCurses, ModCurseEntry> TreatsCurses { get; } = new Dictionary<RogueLikeMode.TreatsCurses, ModCurseEntry>();

		public Dictionary<RogueLikeMode.Perks, ModPerkEntry> Perks { get; } = new Dictionary<RogueLikeMode.Perks, ModPerkEntry>();
	}

	/// <summary>
	/// Holds all content, for all mods, including non-persistents.
	/// </summary>

	internal class GlobalLibrary : PersistentLibrary
	{
		public Dictionary<string, Dictionary<string, CommandParser>> Commands { get; } = new Dictionary<string, Dictionary<string, CommandParser>>();

		public Dictionary<int, ModAudioEntry> Audio { get; } = new Dictionary<int, ModAudioEntry>();

		public Dictionary<string, string> VanillaMusicRedirects { get; } = new Dictionary<string, string>();

		public Dictionary<Level.ZoneEnum, ModLevelEntry> Levels { get; } = new Dictionary<Level.ZoneEnum, ModLevelEntry>();
	}
}