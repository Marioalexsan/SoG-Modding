using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;

namespace SoG.Modding
{
    public static class Utils
    {
        /// <summary>
        /// Tries to create a directory. This method ignores exceptions thrown (if any).
        /// </summary>

        public static void TryCreateDirectory(string name)
        {
            try
            {
                Directory.CreateDirectory(name);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Tries to load a Texture2D using the provided ContentManager and AssetPath and returns it if successful.
        /// If an exception is thrown during load, RenderMaster.txNullTex is returned.
        /// </summary>

        public static Texture2D TryLoadTex(string assetPath, ContentManager manager)
        {
            try
            {
                return manager.Load<Texture2D>(assetPath);
            }
            catch (Exception e)
            {
                GrindScript.Logger.Warn($"Failed to load Texture2D. Exception message: {e.Message}", source: "TryLoadTex");
                return RenderMaster.txNullTex;
            }
        }

        /// <summary>
        /// Tries to load a WaveBank using the provided path and AudioEngine, and returns it if successful.
        /// If an exception is thrown during load, null is returned.
        /// </summary>

        public static WaveBank TryLoadWaveBank(string assetPath, AudioEngine engine)
        {
            try
            {
                return new WaveBank(engine, assetPath);
            }
            catch (Exception e)
            {
                GrindScript.Logger.Warn($"Failed to load WaveBank. Exception message: {e.Message}", source: "TryLoadWaveBank");
                return null;
            }
        }

        /// <summary>
        /// Tries to load a SoundBank using the provided path and AudioEngine, and returns it if successful.
        /// If an exception is thrown during load, null is returned.
        /// </summary>

        public static SoundBank TryLoadSoundBank(string assetPath, AudioEngine engine)
        {
            try
            {
                return new SoundBank(engine, assetPath);
            }
            catch (Exception e)
            {
                GrindScript.Logger.Warn($"Failed to load SoundBank. Exception message: {e.Message}", source: "TryLoadSoundBank");
                return null;
            }
        }

        /// <summary>
        /// Splits an audio ID into separate pieces. Returns true on success.
        /// </summary>

        internal static bool SplitGSAudioID(string ID, out int entryID, out bool isMusic, out int cueID)
        {
            entryID = -1;
            isMusic = false;
            cueID = -1;

            if (!ID.StartsWith("GS_"))
                return false;

            string[] words = ID.Remove(0, 3).Split('_');

            if (words.Length != 2 || !(words[1][0] == 'M' || words[1][0] == 'S'))
                return false;

            try
            {
                entryID = int.Parse(words[0]);
                isMusic = words[1][0] == 'M';
                cueID = int.Parse(words[1].Substring(1));
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves a new Cue for the given modded audio ID.
        /// </summary>

        public static Cue GetEffectCue(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];
            return entry.effectsSoundBank.GetCue(entry.effectIDToName[cueID]);
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public static SoundBank GetEffectSoundBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];

            if (entry == null)
                return null;

            return entry.effectsSoundBank;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public static WaveBank GetEffectWaveBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && !isMusic))
                return null;

            return ModLibrary.Global.ModAudio[entryID]?.effectsWaveBank;
        }

        /// <summary>
        /// Retrieves the SoundBank associated with the given modded audio ID.
        /// </summary>

        public static SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out _);
            if (!(success && isMusic))
                return null;

            return ModLibrary.Global.ModAudio[entryID]?.musicSoundBank;
        }

        /// <summary>
        /// Retrieves the WaveBank associated with the given modded audio ID.
        /// </summary>

        public static string GetMusicWaveBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];

            if (!entry.musicIDToName.TryGetValue(cueID, out string cueName))
                return null;

            if (!entry.musicNameToBank.TryGetValue(cueName, out string bank))
                return null;

            return bank;
        }

        /// <summary>
        /// Checks whenever the given name may represent a persistent WaveBank. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>

        public static bool IsUniversalMusicBank(string bank)
        {
            if (bank == "UniversalMusic")
                return true;

            foreach (var kvp in ModLibrary.Global.ModAudio)
            {
                if (kvp.Value.owner.GetType().Name == bank)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whenever the given WaveBank is persistent. <para/>
        /// Persistent WaveBanks are never unloaded.
        /// </summary>

        public static bool IsUniversalMusicBank(WaveBank bank)
        {
            if (bank == null)
                return false;

            foreach (var kvp in ModLibrary.Global.ModAudio)
            {
                if (kvp.Value.universalMusicWaveBank == bank)
                    return true;
            }

            FieldInfo universalWaveBankField = typeof(SoundSystem).GetTypeInfo().GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bank == universalWaveBankField.GetValue(GrindScript.Game.xSoundSystem))
                return true;

            return false;
        }
    }
}
