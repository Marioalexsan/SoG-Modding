using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SoG.Modding
{
    internal static partial class Patches
    {
        private static void OnGame1Initialize()
        {
            typeof(GrindScript).GetTypeInfo().GetPrivateStaticMethod("Initialize").Invoke(null, new object[0]);
        }

        private static bool OnChatParseCommand(string command, string message, int connection)
        {
            string[] words = command.Split(':');
            if (words.Length < 2)
                return false; // Is probably a vanilla command

            string target = words[0];
            string trueCommand = command.Substring(command.IndexOf(':') + 1);

            if (!ModLibrary.Global.ModCommands.TryGetValue(target, out var parsers))
            {
                CAS.AddChatMessage("[GrindScript] Unknown mod!");
                return true;
            }
            if (!parsers.TryGetValue(trueCommand, out var parser))
            {
                if (trueCommand == "Help")
                {
                    OnChatParseCommand("GScript:Help", target, connection);
                    return true;
                }

                CAS.AddChatMessage($"[{target}] Unknown command!");
                return true;
            }

            GrindScript.Logger.Info($"Parsed command {target} : {trueCommand}, argument list: {message}");
            parser(message, connection);

            return true;
        }

        private static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.Global.ModItems[enType].itemInfo;
            __result = details.vanilla;
            __result.txDisplayImage = Utils.TryLoadTex(details.resourcePath, details.managerToUse);

            return false;
        }

        private static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.Global.ModItems[enType].itemInfo;
            string trueShadowTex = details.shadowPath != "" ? details.shadowPath : BaseScript.SoGPath + "Items/DropAppearance/hartass02";
            ItemDescription xDesc = details.vanilla;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            __result.xRenderComponent.txTexture = Utils.TryLoadTex(details.resourcePath, details.managerToUse);
            __result.xRenderComponent.txShadowTexture = Utils.TryLoadTex(trueShadowTex, details.managerToUse);
            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };

            return false;
        }

        /// <summary>
        /// For modded equipment, allows retrieving EquipmentInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetEquipmentInfo(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = ModLibrary.Global.ModItems[enType].equipInfo.vanilla;

            return false;
        }

        /// <summary>
        /// For modded facegear, allows retrieving FacegearInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetFacegearInfo(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.Global.ModItems[enType].equipInfo;
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as FacegearInfo;
            __result.atxTextures[0] = Utils.TryLoadTex(modEquip.resourcePath + "/Up", manager);
            __result.atxTextures[1] = Utils.TryLoadTex(modEquip.resourcePath + "/Right", manager);
            __result.atxTextures[2] = Utils.TryLoadTex(modEquip.resourcePath + "/Down", manager);
            __result.atxTextures[3] = Utils.TryLoadTex(modEquip.resourcePath + "/Left", manager);

            return false;
        }

        /// <summary>
        /// For modded hats, allows retrieving HatInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetHatInfo(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModEquip modEquip = ModLibrary.Global.ModItems[enType].equipInfo;
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as HatInfo;
            __result.xDefaultSet.atxTextures[0] = Utils.TryLoadTex(modEquip.resourcePath + "/Up", manager);
            __result.xDefaultSet.atxTextures[1] = Utils.TryLoadTex(modEquip.resourcePath + "/Right", manager);
            __result.xDefaultSet.atxTextures[2] = Utils.TryLoadTex(modEquip.resourcePath + "/Down", manager);
            __result.xDefaultSet.atxTextures[3] = Utils.TryLoadTex(modEquip.resourcePath + "/Left", manager);
            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = modEquip.resourcePath + "/" + modEquip.hatAltSetResources[kvp.Key] + "/";
                kvp.Value.atxTextures[0] = Utils.TryLoadTex(altPath + "/Up", manager);
                kvp.Value.atxTextures[1] = Utils.TryLoadTex(altPath + "/Right", manager);
                kvp.Value.atxTextures[2] = Utils.TryLoadTex(altPath + "/Down", manager);
                kvp.Value.atxTextures[3] = Utils.TryLoadTex(altPath + "/Left", manager);
            }

            return false;
        }

        /// <summary>
        /// For modded weapons, allows retrieving WeaponInfo from GrindScript's internals.
        /// </summary>

        private static bool OnGetWeaponInfo(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            __result = ModLibrary.Global.ModItems[enType].equipInfo.vanilla as WeaponInfo;

            return false;
        }

        /// <summary>
        /// Patches <see cref="WeaponAssets.WeaponContentManager.LoadBatch(Dictionary{ushort, string})"/>.
        /// Allows use of modded assets. Modded weapons have shorter asset paths ("/Weapons" is removed).
        /// </summary>

        private static bool OnLoadBatch(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            if (!__instance.enType.IsModItem())
                return true;

            ContentManager managerToUse = ModLibrary.Global.ModItems[__instance.enType].equipInfo.managerToUse;
            if (managerToUse != null)
                __instance.contWeaponContent.RootDirectory = managerToUse.RootDirectory;

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                int start = kvp.Value.IndexOf('<') + 1;
                int end = kvp.Value.IndexOf('>');
                string resourcePath = kvp.Value.Substring(start, end - start);

                string texPath = kvp.Value;
                texPath = texPath.Replace($"Weapons/<{resourcePath}>/", "");
                texPath = texPath.Replace("Sprites/Heroes/OneHanded/", resourcePath + "/");
                texPath = texPath.Replace("Sprites/Heroes/Charge/OneHand/", resourcePath + "/1HCharge/");
                texPath = texPath.Replace("Sprites/Heroes/TwoHanded/", resourcePath + "/");
                texPath = texPath.Replace("Sprites/Heroes/Charge/TwoHand/", resourcePath + "/2HCharge/");

                __instance.ditxWeaponTextures.Add(kvp.Key, Utils.TryLoadTex(texPath, __instance.contWeaponContent));
            }

            return false;
        }

        /// <summary>
        /// Patches <see cref="Game1._Animations_GetAnimationSet(PlayerView, string, string, bool, bool, bool, bool)"/>.
        /// Allows use of modded assets. Modded shields have shorter asset paths ("/Shields" is removed).
        /// </summary>

        private static bool OnGetAnimationSet(PlayerView xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bCustomHat, bool bWithShield, bool bWeaponOnTop, ref PlayerAnimationTextureSet __result)
        {
            // sAttackPath = "" has been removed

            __result = new PlayerAnimationTextureSet() { bWeaponOnTop = bWeaponOnTop };

            ContentManager VanillaContent = RenderMaster.contPlayerStuff;
            __result.txBase = Utils.TryLoadTex($"Sprites/Heroes/{sAnimation}/{sDirection}", VanillaContent);

            string resource;
            if (bWithShield && xPlayerView.xEquipment.DisplayShield != null && (resource = xPlayerView.xEquipment.DisplayShield.sResourceName) != "")
            {
                var enType = xPlayerView.xEquipment.DisplayShield.enItemType;
                bool modItem = enType.IsModItem();

                if (modItem)
                {
                    var managerToUse = ModLibrary.Global.ModItems[enType].equipInfo.managerToUse;
                    string path = $"{resource}/{sAnimation}/{sDirection}";
                    __result.txShield = Utils.TryLoadTex(path, managerToUse);
                }
                else
                {
                    var managerToUse = VanillaContent;
                    string path = $"Sprites/Heroes/{sAnimation}/Shields/{resource}/{sDirection}";
                    __result.txShield = Utils.TryLoadTex(path, managerToUse);
                }
            }

            if (bWithWeapon)
                __result.txWeapon = RenderMaster.txNullTex;

            return false; // Never executes the original
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlaySong"/>.
        /// Vanilla song that have a redirect defined are replaced with their modded counterpart.
        /// </summary>

        private static void OnPlaySong(ref string sSongName, bool bFadeIn)
        {
            string audioIDToUse = sSongName;
            if (!audioIDToUse.StartsWith("GS_") && ModLibrary.Global.VanillaMusicRedirects.ContainsKey(audioIDToUse))
                audioIDToUse = ModLibrary.Global.VanillaMusicRedirects[audioIDToUse];

            sSongName = audioIDToUse;
        }

        /// <summary>
        /// Replaces method <see cref="SoundSystem.ChangeSongRegionIfNecessary"/>.
        /// Allows using music from modded sources.
        /// </summary>

        private static bool OnChangeSongRegionIfNecessary(ref SoundSystem __instance, string sSongName)
        {
            // This will probably cause you brain and eye damage if you read it

            TypeInfo SoundType = typeof(SoundSystem).GetTypeInfo();

            FieldInfo standbyWaveBanksField = SoundType.GetField("dsxStandbyWaveBanks", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo songRegionMapField = SoundType.GetField("dssSongRegionMap", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo audioEngineField = SoundType.GetField("audioEngine", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fieldMusicWaveBank = SoundType.GetField("musicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo vanillaUniversalMusicField = SoundType.GetField("universalMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fieldLoadedMusicWaveBank = SoundType.GetField("loadedMusicWaveBank", BindingFlags.NonPublic | BindingFlags.Instance);

            SoundSystem soundSystem = __instance;

            bool currentIsModded = Utils.SplitGSAudioID(sSongName, out int entryID, out bool isMusic, out int cueID);

            if (currentIsModded && !isMusic)
                GrindScript.Logger.Warn($"Trying to play modded audio as music, but the audio isn't music! ID: {sSongName}");

            var dsxStandbyWaveBanks = (Dictionary<string, WaveBank>)standbyWaveBanksField.GetValue(soundSystem);
            var dssSongRegionMap = (Dictionary<string, string>)songRegionMapField.GetValue(soundSystem);
            AudioEngine audioEngine = (AudioEngine)audioEngineField.GetValue(GrindScript.Game.xSoundSystem);
            WaveBank vanillaUniversalMusic = (WaveBank)vanillaUniversalMusicField.GetValue(soundSystem);

            ModAudioEntry entry = currentIsModded ? ModLibrary.Global.ModAudio[entryID] : null;
            string cueName = currentIsModded ? entry.musicIDToName[cueID] : sSongName;
            string modBank = currentIsModded ? entry.musicNameToBank[cueName] : dssSongRegionMap[sSongName];

            // All bank names must be unique due to XACT design

            WaveBank currentMusicBank = (WaveBank)fieldMusicWaveBank.GetValue(soundSystem);

            if (Utils.IsUniversalMusicBank(modBank))
            {
                if (currentIsModded && entry.universalMusicWaveBank == null)
                {
                    GrindScript.Logger.Warn($"{sSongName} requested modded UniversalMusic bank, but the bank does not exist!");
                    return false;
                }
                if (currentMusicBank != null && !Utils.IsUniversalMusicBank(currentMusicBank))
                {
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);
                }
                fieldMusicWaveBank.SetValue(soundSystem, currentIsModded ? entry.universalMusicWaveBank : vanillaUniversalMusic);
            }
            else if (soundSystem.sCurrentMusicWaveBank != modBank)
            {
                if (currentMusicBank != null && !Utils.IsUniversalMusicBank(currentMusicBank) && !currentMusicBank.IsDisposed)
                {
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);
                }
                soundSystem.sCurrentMusicWaveBank = modBank;
                if (dsxStandbyWaveBanks.ContainsKey(modBank))
                {
                    fieldMusicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[modBank]);
                    dsxStandbyWaveBanks.Remove(modBank);
                }
                else
                {
                    WaveBank newBank;
                    if (currentIsModded)
                        newBank = new WaveBank(audioEngine, $"{entry.owner.ModContent.RootDirectory}/Sound/{modBank}.xwb");
                    else
                        newBank = new WaveBank(audioEngine, $"{GrindScript.Game.Content.RootDirectory}/Sound/{soundSystem.sCurrentMusicWaveBank}.xwb");

                    fieldLoadedMusicWaveBank.SetValue(soundSystem, newBank);
                    fieldMusicWaveBank.SetValue(soundSystem, null);
                }
                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
                SoundType.GetPrivateInstanceMethod("CheckStandbyBanks").Invoke(soundSystem, new object[] { modBank });
            }
            else if (Utils.IsUniversalMusicBank(currentMusicBank))
            {
                if (dsxStandbyWaveBanks.ContainsKey(soundSystem.sCurrentMusicWaveBank))
                {
                    fieldMusicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[soundSystem.sCurrentMusicWaveBank]);
                    dsxStandbyWaveBanks.Remove(soundSystem.sCurrentMusicWaveBank);
                    return false;
                }

                WaveBank newBank;
                if (currentIsModded)
                    newBank = new WaveBank(audioEngine, $"{entry.owner.ModContent.RootDirectory}/Sound/{modBank}.xwb");
                else
                    newBank = new WaveBank(audioEngine, $"{GrindScript.Game.Content.RootDirectory}/Sound/{soundSystem.sCurrentMusicWaveBank}.xwb");

                fieldLoadedMusicWaveBank.SetValue(soundSystem, newBank);
                fieldMusicWaveBank.SetValue(soundSystem, null);
                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
            }

            return false; // Never returns control to original
        }

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