using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Holds state used for modded content.
    /// </summary>

    internal class ModLibrary
	{
		// Global variable

		internal static ModLibrary GlobalLib { get; } = new ModLibrary();

		// Static Fields

		internal static Dictionary<int, ModAudioEntry> Audio { get; } = new Dictionary<int, ModAudioEntry>();

		internal static Dictionary<string, string> VanillaMusicRedirects { get; } = new Dictionary<string, string>();

		internal static Dictionary<string, Dictionary<string, CommandParser>> Commands { get; } = new Dictionary<string, Dictionary<string, CommandParser>>();

		internal static Dictionary<Level.ZoneEnum, ModLevelEntry> Levels { get; } = new Dictionary<Level.ZoneEnum, ModLevelEntry>();

		// Instance Fields
		// GlobalLib contains instance objects for all mods.
		// A specific mod's library contains its own defined things, and is used for Saving and Loading.

		internal Dictionary<ItemCodex.ItemTypes, ModItemEntry> Items { get; } = new Dictionary<ItemCodex.ItemTypes, ModItemEntry>();

		internal Dictionary<RogueLikeMode.TreatsCurses, ModCurseEntry> TreatsCurses { get; } = new Dictionary<RogueLikeMode.TreatsCurses, ModCurseEntry>();

		internal Dictionary<RogueLikeMode.Perks, ModPerkEntry> Perks { get; } = new Dictionary<RogueLikeMode.Perks, ModPerkEntry>();
	}
}