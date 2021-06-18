using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SoG.Modding
{
    /// <summary>
    /// Used to define custom audio added by a mod.
    /// While it is called a Builder, it doesn't create anything, but updates a mod's Audio entry instead.
    /// </summary>
    
    public class ModAudioBuilder
    {
        readonly HashSet<string> effectCues = new HashSet<string>();
        readonly Dictionary<string, HashSet<string>> regionCues = new Dictionary<string, HashSet<string>>();

        public ModAudioBuilder AddEffects(params string[] effects)
        {
            foreach (var audio in effects)
                effectCues.Add(audio);
            return this;
        }

        public ModAudioBuilder AddMusicForRegion(string wavebank, params string[] music)
        {
            var setToUpdate = regionCues.TryGetValue(wavebank, out var set) ? set : regionCues[wavebank] = new HashSet<string>();
            foreach (var audio in music)
                setToUpdate.Add(audio);
            return this;
        }

        internal void UpdateExistingEntry(string assetPath, int ID)
        {
            ModAudioEntry entry = ModLibrary.Global.ModAudio[ID];

            AudioEngine audioEngine;
            try
            {
                audioEngine = (AudioEngine)typeof(SoundSystem).GetField("audioEngine", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GrindScript.Game.xSoundSystem);
            }
            catch (Exception e)
            {
                GrindScript.Logger.Error($"Failed to retrieve SoG's AudioSystem! Exception message: {e.Message}", source: "ModAudioBuilder");
                return;
            }

            if (entry.previouslyBuilt)
            {
                GrindScript.Logger.Warn($"Audio Entry {ID} is being rebuilt by a mod! This can cause issues later on, so try to avoid it if possible.");

                if (entry.effectsSoundBank != null)
                {
                    if (!entry.effectsSoundBank.IsDisposed)
                        entry.effectsSoundBank.Dispose();
                    entry.effectsSoundBank = null;
                }

                if (entry.effectsWaveBank != null)
                {
                    if (!entry.effectsWaveBank.IsDisposed)
                        entry.effectsWaveBank.Dispose();
                    entry.effectsWaveBank = null;
                }

                if (entry.musicSoundBank != null)
                {
                    if (!entry.musicSoundBank.IsDisposed)
                        entry.musicSoundBank.Dispose();
                    entry.musicSoundBank = null;
                }

                if (entry.universalMusicBank != null)
                {
                    if (!entry.universalMusicBank.IsDisposed)
                        entry.universalMusicBank.Dispose();
                    entry.universalMusicBank = null;
                }
            }
            entry.previouslyBuilt = true;

            // Assign modlocal effect IDs
            Dictionary<int, string> effectIDToCue = new Dictionary<int, string>();
            int effectID = 0;
            foreach (var effect in effectCues)
                effectIDToCue[effectID++] = effect;

            string modName = entry.owner.GetType().Name;

            // Assign modlocal music IDs
            Dictionary<int, string> musicIDToCue = new Dictionary<int, string>();
            Dictionary<string, string> cueToWaveBank = new Dictionary<string, string>();
            int musicID = 0;
            foreach (var kvp in regionCues)
            {
                foreach (var music in kvp.Value)
                {
                    cueToWaveBank[music] = kvp.Key;
                    musicIDToCue[musicID++] = music;
                }
                if (!kvp.Key.StartsWith(modName))
                {
                    GrindScript.Logger.Warn($"Music WaveBank {kvp.Key} from mod {modName} does not begin with the mod's name, so it's likely that its name is not unique!");
                    GrindScript.Logger.Warn("Keep in mind that if a name clash occurs, audio may be bugged!");
                }
            }

            
            entry.effectsWaveBank = Utils.TryLoadWaveBank(assetPath + "/Sound/" + modName + "Effects.xwb", audioEngine);
            entry.effectsSoundBank = Utils.TryLoadSoundBank(assetPath + "/Sound/" + modName + "Effects.xsb", audioEngine);
            entry.musicSoundBank = Utils.TryLoadSoundBank(assetPath + "/Sound/" + modName + "Music.xsb", audioEngine);
            entry.universalMusicBank = Utils.TryLoadWaveBank(assetPath + "/Sound/" + modName + ".xwb", audioEngine);
            entry.effectIDToEffect = effectIDToCue;
            entry.musicIDToMusic = musicIDToCue;
            entry.musicToWaveBank = cueToWaveBank;
        }
    }
}
