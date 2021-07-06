using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Core;

namespace SoG.Modding.Patches
{
    /// <summary>
    /// Provides simple access points for transpilers to call.
    /// This class should not be used for other things!
    /// </summary>
    internal static class TranspilerAP
    {
        public static string GetCueName(string ID)
        {
            return APIGlobals.API.AudioAPI.GetCueName(ID);
        }

        public static SoundBank GetEffectSoundBank(string ID)
        {
            return APIGlobals.API.AudioAPI.GetEffectSoundBank(ID);
        }

        public static SoundBank GetMusicSoundBank(string ID)
        {
            return APIGlobals.API.AudioAPI.GetMusicSoundBank(ID);
        }

        public static SpriteBatch SpriteBatch => typeof(Game1).GetField("spriteBatch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(APIGlobals.Game) as SpriteBatch;

        public static TCMenuWorker TCMenuWorker => APIGlobals.API.GetWorker<TCMenuWorker>();
    }
}
