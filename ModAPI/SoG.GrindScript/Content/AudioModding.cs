using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace SoG.Modding
{
    public static class AudioModding
    {
        /// <summary>
        /// Configures custom audio for the mod, using the config provided. <para/>
        /// After configuring the audio, music and effect IDs can be obtained with <see cref="GetEffectID"/> and <see cref="GetMusicID"/>.
        /// </summary>

        public static void ConfigureModAudio(BaseScript owner, AudioConfig cfg)
        {
            if (cfg == null || owner == null)
            {
                GrindScript.Logger.Warn("Can't create audio due to owner or builder being null!");
                return;
            }

            string assetPath = owner.ModPath;
            int ID = owner._audioID;
            ModAudioEntry entry = ModLibrary.Audio[ID];

            AudioEngine audioEngine = typeof(SoundSystem).GetField("audioEngine", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GrindScript.Game.xSoundSystem) as AudioEngine;

            if (entry.isReady)
            {
                GrindScript.Logger.Warn($"Audio Entry {ID} is being redefined for a second time! This may cause issues.");

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

                if (entry.universalMusicWaveBank != null)
                {
                    if (!entry.universalMusicWaveBank.IsDisposed)
                        entry.universalMusicWaveBank.Dispose();
                    entry.universalMusicWaveBank = null;
                }
            }
            entry.isReady = true;

            // Assign indexes to effects
            Dictionary<int, string> effectIDToCue = new Dictionary<int, string>();
            int effectID = 0;
            foreach (var effect in cfg.EffectCues)
                effectIDToCue[effectID++] = effect;

            string modName = entry.owner.GetType().Name;

            // Assign indexes to music
            Dictionary<int, string> musicIDToCue = new Dictionary<int, string>();
            Dictionary<string, string> cueToWaveBank = new Dictionary<string, string>();
            int musicID = 0;
            foreach (var kvp in cfg.RegionCues)
            {
                foreach (var music in kvp.Value)
                {
                    cueToWaveBank[music] = kvp.Key;
                    musicIDToCue[musicID++] = music;
                }
                if (!kvp.Key.StartsWith(modName))
                    GrindScript.Logger.Warn($"Music WaveBank {kvp.Key} from mod {modName} does not follow the naming convention, and may cause conflicts!");
            }

            string root = Path.Combine(entry.owner.ModContent.RootDirectory, assetPath);

            entry.effectsWaveBank = Utils.TryLoadWaveBank(Path.Combine(root, "Sound", modName + "Effects.xwb"), audioEngine);
            entry.effectsSoundBank = Utils.TryLoadSoundBank(Path.Combine(root, "Sound", modName + "Effects.xsb"), audioEngine);
            entry.musicSoundBank = Utils.TryLoadSoundBank(Path.Combine(root, "Sound", modName + "Music.xsb"), audioEngine);
            entry.universalMusicWaveBank = Utils.TryLoadWaveBank(Path.Combine(root, "Sound", modName + ".xwb"), audioEngine);
            entry.effectIDToName = effectIDToCue;
            entry.musicIDToName = musicIDToCue;
            entry.musicNameToBank = cueToWaveBank;
        }

        /// <summary>
        /// Instructs the SoundSystem to play the target modded music instead of the vanilla music. <para/>
        /// If redirect is the empty string, any existing redirects and cleared.
        /// </summary>

        public static void RedirectVanillaMusic(string vanilla, string redirect)
        {
            var songRegionMapField = (Dictionary<string, string>)typeof(SoundSystem).GetTypeInfo().GetField("dssSongRegionMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GrindScript.Game.xSoundSystem);
            if (!songRegionMapField.ContainsKey(vanilla))
            {
                GrindScript.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = Utils.SplitGSAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = ModLibrary.Audio.ContainsKey(entryID) ? ModLibrary.Audio[entryID] : null;
            string cueName = entry != null && entry.musicIDToName.ContainsKey(cueID) ? entry.musicIDToName[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                GrindScript.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = ModLibrary.VanillaMusicRedirects;
            bool replacing = redirectedSongs.ContainsKey(vanilla);

            if (redirect == "")
            {
                GrindScript.Logger.Info($"Song {vanilla} has been cleared of any redirects.");
                redirectedSongs.Remove(vanilla);
            }
            else
            {
                GrindScript.Logger.Info($"Song {vanilla} is now redirected to {redirect} ({cueName}). {(replacing ? $"Previous redirect was {redirectedSongs[vanilla]}" : "")}");
                redirectedSongs[vanilla] = redirect;
            }
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public static string GetEffectID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                GrindScript.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetEffectID(owner._audioID, cueName);
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public static string GetEffectID(int audioEntryID, string cueName)
        {
            var effects = ModLibrary.Audio[audioEntryID].effectIDToName;
            foreach (var kvp in effects)
            {
                if (kvp.Value == cueName)
                {
                    return $"GS_{audioEntryID}_S{kvp.Key}";
                }
            }
            return "";
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public static string GetMusicID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                GrindScript.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetMusicID(owner._audioID, cueName);
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public static string GetMusicID(int audioEntryID, string cueName)
        {
            var music = ModLibrary.Audio[audioEntryID].musicIDToName;
            foreach (var kvp in music)
            {
                if (kvp.Value == cueName)
                    return $"GS_{audioEntryID}_M{kvp.Key}";
            }
            return "";
        }

        /// <summary>
        /// Gets the cue name based on the modded ID. <para/>
        /// </summary>

        public static string GetCueName(string GSID)
        {
            if (!Utils.SplitGSAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";
            ModAudioEntry entry = ModLibrary.Audio[entryID];
            return isMusic ? entry.musicIDToName[cueID] : entry.effectIDToName[cueID];
        }
    }
}
