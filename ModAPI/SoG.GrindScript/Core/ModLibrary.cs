using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Holds state used for modded content.
    /// </summary>

    internal class ModLibrary
	{
		internal static ModLibrary Global { get; private set; } = new ModLibrary();

		internal readonly Dictionary<ItemCodex.ItemTypes, ModItemEntry> Items = new Dictionary<ItemCodex.ItemTypes, ModItemEntry>();
		
		internal readonly Dictionary<int, ModAudioEntry> Audio = new Dictionary<int, ModAudioEntry>();
		
		internal readonly Dictionary<string, string> VanillaMusicRedirects = new Dictionary<string, string>(); // This can redirect vanilla songs, i.e. "BossFight" to "GS_1_M1" or something

		internal readonly Dictionary<string, Dictionary<string, CommandParser>> Commands = new Dictionary<string, Dictionary<string, CommandParser>>();

		internal readonly Dictionary<Level.WorldRegion, WorldRegionEntry> WorldRegions = new Dictionary<Level.WorldRegion, WorldRegionEntry>();

		internal readonly Dictionary<Level.ZoneEnum, ModLevelEntry> Levels = new Dictionary<Level.ZoneEnum, ModLevelEntry>();
	}
}