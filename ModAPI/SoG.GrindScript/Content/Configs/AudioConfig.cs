﻿using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SoG.Modding.Content.Configs
{
    /// <summary>
    /// Used to define custom audio added by a mod.
    /// Its action is tu update a mod's Audio entry.
    /// </summary>

    public class AudioConfig
    {
        /// <summary> 
        /// The cues contained within a mod's music WaveBank.
        /// Together, all cues should also be contained within the mod's music SoundBank.
        /// If a WaveBank has the same file name as the mod, it is never unloaded from the game.
        /// </summary>

        public Dictionary<string, HashSet<string>> RegionCues { get; } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// The cues contained within a mod's effects SoundBank and WaveBank. 
        /// </summary>

        public HashSet<string> EffectCues { get; } = new HashSet<string>();

        /// <summary>
        /// Adds effect cues to the effect list.
        /// </summary>

        public AudioConfig AddEffects(params string[] effects)
        {
            foreach (var audio in effects)
                EffectCues.Add(audio);
            return this;
        }

        /// <summary>
        /// Adds music cues for the specified bank.
        /// </summary>

        public AudioConfig AddMusicForRegion(string wavebank, params string[] music)
        {
            var setToUpdate = RegionCues.TryGetValue(wavebank, out var set) ? set : RegionCues[wavebank] = new HashSet<string>();
            foreach (var audio in music)
                setToUpdate.Add(audio);
            return this;
        }
    }
}
