using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace SoG.Modding
{
    internal class ModAudioEntry
    {
        public BaseScript owner;

        public int allocatedID;

        public bool previouslyBuilt = false;

        public SoundBank effectsSoundBank; // Named as "<Mod Class Name>Effects.xsb", i.e. "FeatureExampleEffects.xsb"

        public WaveBank effectsWaveBank; // Named as "<Mod Class Name>Music.xwb", i.e. "FeatureExampleEffects.xwb"

        public SoundBank musicSoundBank; // Named as "<Mod Class Name>Music.xsb", i.e. "FeatureExampleMusic.xsb"

        public WaveBank universalMusicBank; // Named as "<Mod Class Name>.xwb", i.e. "FeatureExample.xwb"

        public Dictionary<int, string> effectIDToEffect = new Dictionary<int, string>();

        public Dictionary<int, string> musicIDToMusic = new Dictionary<int, string>();

        public Dictionary<string, string> musicToWaveBank = new Dictionary<string, string>();
    }
}
