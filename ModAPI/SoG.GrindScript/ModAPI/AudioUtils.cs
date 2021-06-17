using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    class AudioUtils
    {
        /// <summary>
        /// Splits a cue ID into separate pieces. Returns true if it succeeded, false if the ID is malformed.
        /// The format for a cue ID is "GS_entryID_[one of M or S]_cueID
        /// </summary>

        internal static bool SplitGSAudioID(string ID, out int entryID, out bool isMusic, out int cueID)
        {
            entryID = -1;
            isMusic = false;
            cueID = -1;

            if (!ID.StartsWith("GS_"))
                return false;

            string[] words = ID.Remove(0, 3).Split('_');

            if (words.Length != 2)
                return false;

            if (!(words[1][0] == 'M' || words[1][0] == 'S'))
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

        // So that transpilers don't have too many headaches
        internal static int GetGSAudioID(string ID)
        {
            SplitGSAudioID(ID, out int entryID, out _, out _);
            return entryID;
        }

        public static Cue GetEffectCue(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];
            return entry.effectsSoundBank.GetCue(entry.effectIDToEffect[cueID]);
        }

        public static SoundBank GetEffectSoundBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];

            if (entry == null)
                return null;

            return entry.effectsSoundBank;
        }

        public static WaveBank GetEffectWaveBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && !isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];

            if (entry == null)
                return null;

            return entry.effectsWaveBank;
        }

        public static SoundBank GetMusicSoundBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];

            if (entry == null)
                return null;

            return entry.musicSoundBank;
        }

        public static string GetMusicWaveBank(string audioID)
        {
            bool success = SplitGSAudioID(audioID, out int entryID, out bool isMusic, out int cueID);
            if (!(success && isMusic))
                return null;

            var entry = ModLibrary.Global.ModAudio[entryID];

            if (!entry.musicIDToMusic.TryGetValue(cueID, out string cueName))
                return null;

            if (!entry.musicToWaveBank.TryGetValue(cueName, out string bank))
                return null;

            return bank;
        }

        public static bool IsUniversalMusicBank(WaveBank bank)
        {
            if (bank == null)
                return false;

            foreach (var kvp in ModLibrary.Global.ModAudio)
            {
                if (kvp.Value.universalMusicBank == bank)
                    return true;
            }

            FieldInfo universalWaveBankField = typeof(SoundSystem).GetTypeInfo().GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bank == universalWaveBankField.GetValue(GrindScript.Game.xSoundSystem))
                return true;

            return false;
        }
    }
}
