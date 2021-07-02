using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoG.Modding.Core;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using SoG.Modding.Tools;

namespace SoG.Modding.Patches
{
    internal static partial class PatchCollection
    {
        /// <summary>
        /// Runs after <see cref="Game1._Saving_SaveCharacterToFile(int)"/>.
        /// </summary>

        private static void PostCharacterSave(int iFileSlot)
        {
            string ext = SaveModding.ModExt;

            PlayerView player = ModGlobals.Game.xLocalPlayer;
            string appData = ModGlobals.Game.sAppData;

            int carousel = player.iSaveCarousel - 1;
            if (carousel < 0)
                carousel += 5;

            string backupPath = "";

            string chrFile = $"{appData}Characters/" + $"{iFileSlot}.cha{ext}";

            if (File.Exists(chrFile))
            {
                if (player.sSaveableName == "")
                {
                    player.sSaveableName = player.sNetworkNickname;
                    foreach (char c in Path.GetInvalidFileNameChars())
                        player.sSaveableName = player.sSaveableName.Replace(c, ' ');
                }

                backupPath = $"{appData}Backups/" + $"{player.sSaveableName}_{player.xJournalInfo.iCollectorID}{iFileSlot}/";
                Utils.TryCreateDirectory(backupPath);

                File.Copy(chrFile, backupPath + $"auto{carousel}.cha{ext}", overwrite: true);

                string wldFile = $"{appData}Worlds/" + $"{iFileSlot}.wld{ext}";
                if (File.Exists(wldFile))
                {
                    File.Copy(wldFile, backupPath + $"auto{carousel}.wld{ext}", overwrite: true);
                }
            }

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{chrFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                ModGlobals.Log.Info($"Saving mod character {iFileSlot}...");
                ModGlobals.API.SaveAPI.SaveModCharacter(bw);
            }

            try
            {
                File.Copy($"{chrFile}.temp", chrFile, overwrite: true);
                if (backupPath != "")
                {
                    File.Copy($"{chrFile}.temp", backupPath + $"latest.cha{ext}", overwrite: true);
                }
                File.Delete($"{chrFile}.temp");
            }
            catch { }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Saving_SaveWorldToFile(int)"/>.
        /// </summary>

        private static void PostWorldSave(int iFileSlot)
        {
            string ext = SaveModding.ModExt;

            PlayerView player = ModGlobals.Game.xLocalPlayer;
            string appData = ModGlobals.Game.sAppData;

            string backupPath = "";
            string chrFile = $"{appData}Characters/" + $"{iFileSlot}.cha{ext}";
            string wldFile = $"{appData}Worlds/" + $"{iFileSlot}.wld{ext}";

            if (File.Exists(chrFile))
            {
                if (player.sSaveableName == "")
                {
                    player.sSaveableName = player.sNetworkNickname;
                    foreach (char c in Path.GetInvalidFileNameChars())
                        player.sSaveableName = player.sSaveableName.Replace(c, ' ');
                }

                backupPath = $"{appData}Backups/" + $"{player.sSaveableName}_{player.xJournalInfo.iCollectorID}{iFileSlot}/";
                Utils.TryCreateDirectory(backupPath);
            }

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{wldFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                ModGlobals.Log.Info($"Saving mod world {iFileSlot}...");
                ModGlobals.API.SaveAPI.SaveModWorld(bw);
            }

            try
            {
                File.Copy($"{wldFile}.temp", wldFile, overwrite: true);
                if (backupPath != "" && iFileSlot != 100)
                {
                    File.Copy($"{wldFile}.temp", backupPath + $"latest.wld{ext}", overwrite: true);
                }
                File.Delete($"{wldFile}.temp");
            }
            catch { }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Saving_SaveRogueToFile(string)"/>
        /// </summary>

        private static void PostArcadeSave()
        {
            string ext = SaveModding.ModExt;

            bool other = CAS.IsDebugFlagSet("OtherArcadeMode");
            string savFile = ModGlobals.Game.sAppData + $"arcademode{(other ? "_other" : "")}.sav{ext}";

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{savFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                ModGlobals.Log.Info($"Saving mod arcade...");
                ModGlobals.API.SaveAPI.SaveModArcade(bw);
            }

            File.Copy($"{savFile}.temp", savFile, overwrite: true);
            File.Delete($"{savFile}.temp");
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadCharacterFromFile(int, bool)"/>
        /// </summary>

        private static void PostCharacterLoad(int iFileSlot, bool bAppearanceOnly)
        {
            string ext = SaveModding.ModExt;

            string chrFile = ModGlobals.Game.sAppData + "Characters/" + $"{iFileSlot}.cha{ext}";

            if (!File.Exists(chrFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(chrFile, FileMode.Open, FileAccess.Read)))
            {
                ModGlobals.Log.Info($"Loading mod character {iFileSlot}...");
                ModGlobals.API.SaveAPI.LoadModCharacter(br);
            }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadWorldFromFile(int)"/>
        /// </summary>

        private static void PostWorldLoad(int iFileSlot)
        {
            string ext = SaveModding.ModExt;

            string wldFile = ModGlobals.Game.sAppData + "Worlds/" + $"{iFileSlot}.wld{ext}";

            if (!File.Exists(wldFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(wldFile, FileMode.Open, FileAccess.Read)))
            {
                ModGlobals.Log.Info($"Loading mod world {iFileSlot}...");
                ModGlobals.API.SaveAPI.LoadModWorld(br);
            }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadRogueFile(string)"/>
        /// </summary>

        private static void PostArcadeLoad()
        {
            string ext = SaveModding.ModExt;

            if (RogueLikeMode.LockedOutDueToHigherVersionSaveFile) return;

            bool other = CAS.IsDebugFlagSet("OtherArcadeMode");
            string savFile = ModGlobals.Game.sAppData + $"arcademode{(other ? "_other" : "")}.sav{ext}";

            if (!File.Exists(savFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(savFile, FileMode.Open, FileAccess.Read)))
            {
                ModGlobals.Log.Info($"Loading mod arcade...");
                ModGlobals.API.SaveAPI.LoadModArcade(br);
            }
        }
    }
}
