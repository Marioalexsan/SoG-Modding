using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SoG.Modding
{
    internal static partial class Patches
    {
        /// <summary>
        /// Runs after <see cref="Game1._Saving_SaveCharacterToFile(int)"/>.
        /// </summary>

        private static void PostCharacterSave(int iFileSlot)
        {
            string ext = ModSaveLoad.ModExt;

            PlayerView player = GrindScript.Game.xLocalPlayer;
            string appData = GrindScript.Game.sAppData;

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
                GrindScript.Logger.Info($"Saving mod character {iFileSlot}...");
                ModSaveLoad.SaveModCharacter(bw);
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
            string ext = ModSaveLoad.ModExt;

            PlayerView player = GrindScript.Game.xLocalPlayer;
            string appData = GrindScript.Game.sAppData;

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
                GrindScript.Logger.Info($"Saving mod world {iFileSlot}...");
                ModSaveLoad.SaveModWorld(bw);
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
            string ext = ModSaveLoad.ModExt;

            bool other = CAS.IsDebugFlagSet("OtherArcadeMode");
            string savFile = GrindScript.Game.sAppData + $"arcademode{(other ? "_other" : "")}.sav{ext}";

            using (BinaryWriter bw = new BinaryWriter(new FileStream($"{savFile}.temp", FileMode.Create, FileAccess.Write)))
            {
                GrindScript.Logger.Info($"Saving mod arcade...");
                ModSaveLoad.SaveModArcade(bw);
            }

            File.Copy($"{savFile}.temp", savFile, overwrite: true);
            File.Delete($"{savFile}.temp");
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadCharacterFromFile(int, bool)"/>
        /// </summary>

        private static void PostCharacterLoad(int iFileSlot, bool bAppearanceOnly)
        {
            string ext = ModSaveLoad.ModExt;

            string chrFile = GrindScript.Game.sAppData + "Characters/" + $"{iFileSlot}.cha{ext}";

            if (!File.Exists(chrFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(chrFile, FileMode.Open, FileAccess.Read)))
            {
                GrindScript.Logger.Info($"Loading mod character {iFileSlot}...");
                ModSaveLoad.LoadModCharacter(br);
            }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadWorldFromFile(int)"/>
        /// </summary>

        private static void PostWorldLoad(int iFileSlot)
        {
            string ext = ModSaveLoad.ModExt;

            string wldFile = GrindScript.Game.sAppData + "Worlds/" + $"{iFileSlot}.wld{ext}";

            if (!File.Exists(wldFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(wldFile, FileMode.Open, FileAccess.Read)))
            {
                GrindScript.Logger.Info($"Loading mod world {iFileSlot}...");
                ModSaveLoad.LoadModWorld(br);
            }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadRogueFile(string)"/>
        /// </summary>

        private static void PostArcadeLoad()
        {
            string ext = ModSaveLoad.ModExt;

            if (RogueLikeMode.LockedOutDueToHigherVersionSaveFile) return;

            bool other = CAS.IsDebugFlagSet("OtherArcadeMode");
            string savFile = GrindScript.Game.sAppData + $"arcademode{(other ? "_other" : "")}.sav{ext}";

            if (!File.Exists(savFile)) return;

            using (BinaryReader br = new BinaryReader(new FileStream(savFile, FileMode.Open, FileAccess.Read)))
            {
                GrindScript.Logger.Info($"Loading mod arcade...");
                ModSaveLoad.LoadModArcade(br);
            }
        }
    }
}
