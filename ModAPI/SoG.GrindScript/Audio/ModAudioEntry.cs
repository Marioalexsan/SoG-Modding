using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace SoG.Modding
{
    internal class ModAudioEntry
    {
        public BaseScript owner;

        public int allocatedID;

        public bool previouslyBuilt = false;

        public SoundBank effectsSoundBank;

        public WaveBank effectsWaveBank;

        public SoundBank musicSoundBank;

        public WaveBank universalMusicBank;

        public Dictionary<int, string> effectIDToEffect = new Dictionary<int, string>();

        public Dictionary<int, string> musicIDToMusic = new Dictionary<int, string>();

        public Dictionary<string, string> musicToWaveBank = new Dictionary<string, string>();
    }
}
