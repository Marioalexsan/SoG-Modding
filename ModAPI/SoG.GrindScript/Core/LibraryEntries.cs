using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Represents a modded object type that is assigned to a certain mod.
    /// </summary>
    abstract class Entry<IDType> where IDType : struct 
    {
        public BaseScript Owner { get; private set; }

        public IDType GameID { get; private set; }

        public Entry(BaseScript owner, IDType gameID)
        {
            Owner = owner;
            GameID = gameID;
        }
    }

    /// <summary>
    /// PersistentEntries represent objects that are saved by SoG in one way or another.
    /// As a result, additional information is required for ensuring consistency.
    /// </summary>
    abstract class PersistentEntry<IDType> : Entry<IDType> where IDType : struct
    {
        public string ModID { get; private set; }

        public PersistentEntry(BaseScript owner, IDType gameID, string modID)
            : base(owner, gameID)
        {
            ModID = modID;
        }
    }

    /// <summary>
    /// Represents a modded item in the ModLibrary.
    /// The item can act as equipment if EquipData is not null.
    internal class ModItemEntry : PersistentEntry<ItemCodex.ItemTypes>
    {
        public ItemDescription ItemData;

        public EquipmentInfo EquipData; // May be null or a subtype

        public ContentManager Manager;

        public string ItemShadowPath = "";

        public string ItemResourcePath = "";

        public string EquipResourcePath = "";

        public Dictionary<ItemCodex.ItemTypes, string> HatAltSetResourcePaths;

        public ModItemEntry(BaseScript owner, ItemCodex.ItemTypes gameID, string modID)
            : base(owner, gameID, modID) { }
    }

    /// <summary>
    /// Stores modded audio for a mod - an entry is created for each mod upon its creation,
    /// and initialized by the mod if needed.
    /// </summary>
    internal class ModAudioEntry : Entry<int>
    {
        public bool IsReady = false;

        public SoundBank EffectsSB; // "<Mod>Effects.xsb"

        public WaveBank EffectsWB; // "<Mod>Music.xwb"

        public SoundBank MusicSB; //"<Mod>Music.xsb"

        public WaveBank UniversalWB; // "<Mod>.xwb", never unloaded

        public Dictionary<int, string> EffectNames = new Dictionary<int, string>();

        public Dictionary<int, string> MusicNames = new Dictionary<int, string>();

        public Dictionary<string, string> MusicBankNames = new Dictionary<string, string>();

        public ModAudioEntry(BaseScript owner, int audioID)
            : base(owner, audioID) { }
    }
    
    /// <summary>
    /// Represents a modded level in the ModLibrary.
    /// </summary>
    internal class ModLevelEntry : Entry<Level.ZoneEnum>
    {
        public LevelBuilder Builder;

        public LevelLoader Loader;

        public Level.WorldRegion Region;

        public ModLevelEntry(BaseScript owner, Level.ZoneEnum gameID)
            : base(owner, gameID) { }
    }

    /// <summary>
    /// Represents a modded treat or curse in the ModLibrary.
    /// Treats are functionally identical to Curses, but appear in a different menu.
    /// </summary>
    internal class ModCurseEntry : PersistentEntry<RogueLikeMode.TreatsCurses>
    {
        public bool IsTreat = false;

        public string NameHandle = "";

        public string DescriptionHandle = "";

        public string ResourcePath = "";

        public float ScoreModifier = 0f;

        public ModCurseEntry(BaseScript owner, RogueLikeMode.TreatsCurses gameID, string modID)
            : base(owner, gameID, modID) { }
    }

    /// <summary>
    /// Represents a modded perk in the ModLibrary.
    /// </summary>
    internal class ModPerkEntry : PersistentEntry<RogueLikeMode.Perks>
    {
        public int EssenceCost = 15;

        public string TextEntry = "";

        public string ResourcePath = "";

        public Action<PlayerView> Activator;

        public ModPerkEntry(BaseScript owner, RogueLikeMode.Perks gameID, string modID)
            : base(owner, gameID, modID) { }
    }
}
