using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace SoG.Modding
{
    /// <summary>
    /// Represents a modded object that is persisted by SoG
    /// ISaveables need additional data attached when saving so that
    /// enum IDs can be identified and restored.
    /// </summary>

    abstract class IPersistentID<IDType> where IDType : Enum
    {
        public BaseScript Owner { get; set; }

        public string ModID { get; set; }

        public IDType GameID { get; set; }
    }

    /// <summary>
    /// Represents a modded item in the ModLibrary.
    /// </summary>

    internal class ModItemEntry : IPersistentID<ItemCodex.ItemTypes>
    {
        public ModItem itemInfo;

        public ModEquip equipInfo;
    }

    /// <summary>
    /// Stores data for a modded item description.
    /// </summary>

    internal class ModItem
    {
        public ItemDescription vanilla;

        public string shadowPath = "";

        public string resourcePath = "";

        public ContentManager managerToUse;
    }

    /// <summary>
    /// Stores data for a modded equipment info.
    /// </summary>

    internal class ModEquip
    {
        public EquipmentInfo vanilla;

        public string resourcePath = "";

        public ContentManager managerToUse;

        public Dictionary<ItemCodex.ItemTypes, string> hatAltSetResources;

        public EquipType equipType;
    }

    /// <summary>
    /// Stores modded audio for a mod - an entry is created for each mod upon its creation, and initialized by the mod if needed
    /// </summary>

    internal class ModAudioEntry
    {
        public BaseScript owner;

        public int allocatedID;

        public bool isReady = false;

        public SoundBank effectsSoundBank; // "<Mod>Effects.xsb", i.e. "FeatureExampleEffects.xsb"

        public WaveBank effectsWaveBank; // "<Mod>Music.xwb", i.e. "FeatureExampleEffects.xwb"

        public SoundBank musicSoundBank; //"<Mod>Music.xsb", i.e. "FeatureExampleMusic.xsb"

        public WaveBank universalMusicWaveBank; // "<Mod>.xwb", i.e. "FeatureExample.xwb". Universal Music is always kept loaded, so only frequently used tracks should be put here.

        public Dictionary<int, string> effectIDToName = new Dictionary<int, string>();

        public Dictionary<int, string> musicIDToName = new Dictionary<int, string>();

        public Dictionary<string, string> musicNameToBank = new Dictionary<string, string>();
    }
    
    internal class ModLevelEntry
    {
        public BaseScript owner;

        public Level.ZoneEnum type;

        public ModLevel levelInfo;
    }

    internal class ModLevel
    {
        public Level.ZoneEnum type;

        public LevelBuilder builder;

        public LevelLoader loader;

        public Level.WorldRegion region;
    }

    internal class ModCurseEntry : IPersistentID<RogueLikeMode.TreatsCurses>
    {
        public bool isTreat = false;

        public string nameHandle = "";

        public string descriptionHandle = "";

        public string resourcePath = "";

        public float scoreModifier = 0f;
    }

    internal class ModPerkEntry : IPersistentID<RogueLikeMode.Perks>
    {
        public int essenceCost;

        public string textEntry;

        public string resourcePath = "";

        public Action<PlayerView> activator;
    }
}
