using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Core;

namespace SoG.Modding.Patches
{
    /// <summary>
    /// Provides simple access points for transpilers to call.
    /// </summary>

    internal static class StaticAccessPoints
    {
        public static string GetCueName(string ID)
        {
            return ModGlobals.API.AudioAPI.GetCueName(ID);
        }

        public static SoundBank GetEffectSoundBank(string ID)
        {
            return ModGlobals.API.AudioAPI.GetEffectSoundBank(ID);
        }

        public static SoundBank GetMusicSoundBank(string ID)
        {
            return ModGlobals.API.AudioAPI.GetMusicSoundBank(ID);
        }
    }
}
