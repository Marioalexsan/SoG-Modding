using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace SoG.Modding
{
    // Stores modded audio for a mod - an entry is created for each mod upon its creation, and initialized by the mod if needed
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
}
