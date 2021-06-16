using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

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
    }
}
