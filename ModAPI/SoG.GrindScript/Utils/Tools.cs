using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using SoG.Modding.Core;

namespace SoG.Modding.Utils
{
    public static class Tools
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
        /// If an exception is thrown during load, GrindScript.MissingTex or RenderMaster.txNullTex is returned.
        /// </summary>
        public static Texture2D TryLoadTex(string assetPath, ContentManager manager)
        {
            try
            {
                return manager.Load<Texture2D>(assetPath);
            }
            catch (Exception e)
            {
                APIGlobals.Logger.Warn(GetShortenedPath(e.Message), source: nameof(TryLoadTex));
                return APIGlobals.API.MissingTex ?? RenderMaster.txNullTex;
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
                APIGlobals.Logger.Warn(GetShortenedPath(e.Message), source: nameof(TryLoadWaveBank));
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
                APIGlobals.Logger.Warn(GetShortenedPath(e.Message), source: nameof(TryLoadSoundBank));
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
        /// Splits message in words, removing any empty results
        /// </summary>
        public static string[] GetArgs(string message)
        {
            return message == null ? new string[0] : message.Split(new char[] { ' ' }, options: StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns a string where common mod paths are replaced with a shortened form.
        /// </summary>
        public static string GetShortenedPath(string path)
        {
            return path
                .Replace('/', '\\')
                .Replace(Directory.GetCurrentDirectory() + @"\Content\ModContent", "(ModContent)")
                .Replace(Directory.GetCurrentDirectory() + @"\Content\Mods", "(Mods)")
                .Replace(Directory.GetCurrentDirectory() + @"\Content", "(Content)")
                .Replace(Directory.GetCurrentDirectory(), "(SoG)");
        }
    }
}
