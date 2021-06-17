using System.Collections.Generic;

namespace SoG.Modding
{
	/// <summary>
	/// Holds modded game content.
	/// </summary>

    internal class ModLibrary
	{
		internal static ModLibrary Global { get; private set; } = new ModLibrary();

		internal readonly Dictionary<ItemCodex.ItemTypes, ModItemEntry> ModItems = new Dictionary<ItemCodex.ItemTypes, ModItemEntry>();
		internal readonly Dictionary<int, ModAudioEntry> ModAudio = new Dictionary<int, ModAudioEntry>();
	}
}