using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using SoG.Modding.Core;
using SoG.Modding.Utils;
using System;
using System.Diagnostics;
using SoG.Modding.Content.Configs;

namespace SoG.Modding.Content
{
    public class AudioModding : ModdingLogic
    {
        public AudioModding(GrindScript modAPI)
            : base(modAPI) { }

        /// <summary>
        /// Configures custom audio for the current mod, using the config provided. <para/>
        /// Config must not be null.
        /// </summary>
        public void CreateAudio(AudioConfig cfg)
        {
            ThrowHelper.ThrowIfNull(cfg);

            BaseScript mod = _modAPI.CurrentModContext;
            if (mod == null)
            {
                _modAPI.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateAudio));
                return;
            }

            string assetPath = mod.ModPath;
            int ID = mod.ModIndex;
            ModAudioEntry entry = _modAPI.Library.Audio[ID];

            AudioEngine audioEngine = typeof(SoundSystem).GetField("audioEngine", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_modAPI.Game.xSoundSystem) as AudioEngine;

            if (entry.IsReady)
            {
                _modAPI.Logger.Warn($"Audio Entry {ID} is being redefined for a second time! This may cause issues.");

                if (entry.EffectsSB != null)
                {
                    if (!entry.EffectsSB.IsDisposed)
                        entry.EffectsSB.Dispose();
                    entry.EffectsSB = null;
                }

                if (entry.EffectsWB != null)
                {
                    if (!entry.EffectsWB.IsDisposed)
                        entry.EffectsWB.Dispose();
                    entry.EffectsWB = null;
                }

                if (entry.MusicSB != null)
                {
                    if (!entry.MusicSB.IsDisposed)
                        entry.MusicSB.Dispose();
                    entry.MusicSB = null;
                }

                if (entry.UniversalWB != null)
                {
                    if (!entry.UniversalWB.IsDisposed)
                        entry.UniversalWB.Dispose();
                    entry.UniversalWB = null;
                }
            }
            entry.IsReady = true;

            // Assign indexes to effects
            Dictionary<int, string> effectIDToCue = new Dictionary<int, string>();
            int effectID = 0;
            foreach (var effect in cfg.EffectCues)
                effectIDToCue[effectID++] = effect;

            string modName = entry.Owner.GetType().Name;

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
                    _modAPI.Logger.Warn($"Music WaveBank {kvp.Key} from mod {modName} does not follow the naming convention, and may cause conflicts!");
            }

            string root = Path.Combine(entry.Owner.ModContent.RootDirectory, assetPath);

            entry.EffectsWB = Utils.Tools.TryLoadWaveBank(Path.Combine(root, "Sound", modName + "Effects.xwb"), audioEngine);
            entry.EffectsSB = Utils.Tools.TryLoadSoundBank(Path.Combine(root, "Sound", modName + "Effects.xsb"), audioEngine);
            entry.MusicSB = Utils.Tools.TryLoadSoundBank(Path.Combine(root, "Sound", modName + "Music.xsb"), audioEngine);
            entry.UniversalWB = Utils.Tools.TryLoadWaveBank(Path.Combine(root, "Sound", modName + ".xwb"), audioEngine);
            entry.EffectNames = effectIDToCue;
            entry.MusicNames = musicIDToCue;
            entry.MusicBankNames = cueToWaveBank;
        }

        /// <summary>
        /// Instructs the SoundSystem to play the target modded music instead of the vanilla music. <para/>
        /// If redirect is the empty string, any existing redirects and cleared.
        /// </summary>

        public void RedirectVanillaMusic(string vanilla, string redirect)
        {
            var songRegionMapField = (Dictionary<string, string>)typeof(SoundSystem).GetTypeInfo().GetField("dssSongRegionMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_modAPI.Game.xSoundSystem);
            if (!songRegionMapField.ContainsKey(vanilla))
            {
                _modAPI.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {vanilla} is not a vanilla music!");
                return;
            }

            bool isModded = Utils.Tools.SplitGSAudioID(redirect, out int entryID, out bool isMusic, out int cueID);
            var entry = _modAPI.Library.Audio.ContainsKey(entryID) ? _modAPI.Library.Audio[entryID] : null;
            string cueName = entry != null && entry.MusicNames.ContainsKey(cueID) ? entry.MusicNames[cueID] : null;

            if ((!isModded || !isMusic || cueName == null) && !(redirect == ""))
            {
                _modAPI.Logger.Warn($"Redirecting {vanilla} to {redirect} is not possible since {redirect} is not a modded music!");
                return;
            }

            var redirectedSongs = _modAPI.Library.VanillaMusicRedirects;
            bool replacing = redirectedSongs.ContainsKey(vanilla);

            if (redirect == "")
            {
                _modAPI.Logger.Info($"Song {vanilla} has been cleared of any redirects.");
                redirectedSongs.Remove(vanilla);
            }
            else
            {
                _modAPI.Logger.Info($"Song {vanilla} is now redirected to {redirect} ({cueName}). {(replacing ? $"Previous redirect was {redirectedSongs[vanilla]}" : "")}");
                redirectedSongs[vanilla] = redirect;
            }
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public string GetEffectID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                _modAPI.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetEffectID(owner.ModIndex, cueName);
        }

        /// <summary>
        /// Gets the ID of the effect that has the given cue name. <para/>
        /// This ID can be used to play effects with methods such as <see cref="SoundSystem.PlayCue"/>.
        /// </summary>

        public string GetEffectID(int audioEntryID, string cueName)
        {
            var effects = _modAPI.Library.Audio[audioEntryID].EffectNames;
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

        public string GetMusicID(BaseScript owner, string cueName)
        {
            if (owner == null)
            {
                _modAPI.Logger.Warn("Can't get sound ID due to owner being null!");
                return "";
            }
            return GetMusicID(owner.ModIndex, cueName);
        }

        /// <summary>
        /// Gets the ID of the music that has the given cue name. <para/>
        /// This ID can be used to play effects with <see cref="SoundSystem.PlaySong"/>.
        /// </summary>

        public string GetMusicID(int audioEntryID, string cueName)
        {
            var music = _modAPI.Library.Audio[audioEntryID].MusicNames;
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

        public string GetCueName(string GSID)
        {
            if (!Utils.Tools.SplitGSAudioID(GSID, out int entryID, out bool isMusic, out int cueID))
                return "";
            ModAudioEntry entry = _modAPI.Library.Audio[entryID];
            return isMusic ? entry.MusicNames[cueID] : entry.EffectNames[cueID];
        }

        /// <summary>
        /// Retrieves a new Cue for the given modded audio ID.
        /// </summary>

        public Cue GetEffectCue(string audioID)
        {
            bool success = Utils.Tools.SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = _modAPI.Library.Audio[entryID];
            return entry.EffectsSB.GetCue(entry.EffectNames[cueID]);
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public SoundBank GetEffectSoundBank(string audioID)
        {
            bool success = Utils.Tools.SplitGSAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            var entry = _modAPI.Library.Audio[entryID];

            if (entry == null)
                return null;

            return entry.EffectsSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public WaveBank GetEffectWaveBank(string audioID)
        {
            bool success = Utils.Tools.SplitGSAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            return _modAPI.Library.Audio[entryID]?.EffectsWB;
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = Utils.Tools.SplitGSAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && isMusic))
                return null;

            return _modAPI.Library.Audio[entryID]?.MusicSB;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public string GetMusicWaveBank(string audioID)
        {
            bool success = Utils.Tools.SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && isMusic))
                return null;

            var entry = _modAPI.Library.Audio[entryID];

            if (!entry.MusicNames.TryGetValue(cueID, out string cueName))
                return null;

            if (!entry.MusicBankNames.TryGetValue(cueName, out string bank))
                return null;

            return bank;
        }

        /// <summary>
        /// Checks whenever the given name may represent a persistent WaveBank. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>

        public bool IsUniversalMusicBank(string bank)
        {
            if (bank == "UniversalMusic")
                return true;

            foreach (var kvp in _modAPI.Library.Audio)
            {
                if (kvp.Value.Owner.GetType().Name == bank)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whenever the given WaveBank is persistent. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>

        public bool IsUniversalMusicBank(WaveBank bank)
        {
            if (bank == null)
                return false;

            foreach (var kvp in _modAPI.Library.Audio)
            {
                if (kvp.Value.UniversalWB == bank)
                    return true;
            }

            FieldInfo universalWaveBankField = typeof(SoundSystem).GetTypeInfo().GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bank == universalWaveBankField.GetValue(_modAPI.Game.xSoundSystem))
                return true;

            return false;
        }
    }
}
