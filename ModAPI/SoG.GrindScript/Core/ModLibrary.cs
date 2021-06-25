using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Holds state used for modded content.
    /// </summary>

    internal class ModLibrary
	{
		internal static ModLibrary Global { get; private set; } = new ModLibrary();

		internal readonly Dictionary<ItemCodex.ItemTypes, ModItemEntry> ModItems = new Dictionary<ItemCodex.ItemTypes, ModItemEntry>();
		
		internal readonly Dictionary<int, ModAudioEntry> ModAudio = new Dictionary<int, ModAudioEntry>();
		
		internal readonly Dictionary<string, string> VanillaMusicRedirects = new Dictionary<string, string>(); // This can redirect vanilla songs, i.e. "BossFight" to "GS_1_M1" or something

		internal readonly Dictionary<string, Dictionary<string, CommandParser>> ModCommands = new Dictionary<string, Dictionary<string, CommandParser>>();
	}
}