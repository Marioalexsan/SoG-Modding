using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;

namespace SoG.Modding
{
    /// <summary>
    /// Contains methods that act as prefix and postfix patches.
    /// </summary>

    public static class Callbacks
    {
        //
        // Mod callbacks
        //

        private static void OnContentLoad()
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.LoadContent();
        }

        private static void OnFinalDrawPrefix()
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnDraw();
        }

        private static void OnPlayerTakeDamagePrefix(ref int iInDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnPlayerDamaged(ref iInDamage, ref byType);
        }

        private static void OnPlayerKilledPrefix()
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnPlayerKilled();
        }

        private static void PostPlayerLevelUp(PlayerView xView)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.PostPlayerLevelUp(xView);
        }

        private static void OnEnemyTakeDamagePrefix(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCTakeDamagePrefix(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCInteractionPrefix(PlayerView xView, NPC xNPC)
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnNPCInteraction(xNPC);
        }

        private static void OnArcadiaLoadPrefix()
        {
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnArcadiaLoad();

            // Just in case it didn't get set before
            GrindScript.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;
        }

        //
        // API Functionality
        //

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
                CAS.AddChatMessage($"[{target}] Unknown command!");
                return true;
            }

            GrindScript.Logger.Info($"Parsed command {target} : {trueCommand}, argument list: {message}");
            parser(message, connection);

            return true;
        }

        private static void OnItemUsePrefix(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead) return;
            foreach (BaseScript mod in GrindScript._loadedScripts)
                mod.OnItemUse(enItem, xView, ref bSend);
        }

        private static bool OnGetItemDescription(ref ItemDescription __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.Global.ModItems[enType].itemInfo;
            __result = details.vanilla;
            __result.txDisplayImage = Utils.TryLoadTex("Items/DropAppearance/" + details.resourceToUse, details.managerToUse);

            return false;
        }

        private static bool OnGetItemInstance(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
                return true;

            ModItem details = ModLibrary.Global.ModItems[enType].itemInfo;
            string trueShadowTex = details.shadowToUse != "" ? details.shadowToUse : "hartass02";
            ItemDescription xDesc = details.vanilla;

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            __result.xRenderComponent.txTexture = Utils.TryLoadTex("Items/DropAppearance/" + details.resourceToUse, details.managerToUse);
            __result.xRenderComponent.txShadowTexture = Utils.TryLoadTex("Items/DropAppearance/" + trueShadowTex, details.managerToUse);
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
            string hatPath = "Sprites/Equipment/Facegear/" + modEquip.resourceToUse + "/";
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as FacegearInfo;
            __result.atxTextures[0] = Utils.TryLoadTex(hatPath + "Up", manager);
            __result.atxTextures[1] = Utils.TryLoadTex(hatPath + "Right", manager);
            __result.atxTextures[2] = Utils.TryLoadTex(hatPath + "Down", manager);
            __result.atxTextures[3] = Utils.TryLoadTex(hatPath + "Left", manager);

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
            string hatPath = "Sprites/Equipment/Hats/" + modEquip.resourceToUse + "/";
            ContentManager manager = modEquip.managerToUse;

            __result = modEquip.vanilla as HatInfo;
            __result.xDefaultSet.atxTextures[0] = Utils.TryLoadTex(hatPath + "Up", manager);
            __result.xDefaultSet.atxTextures[1] = Utils.TryLoadTex(hatPath + "Right", manager);
            __result.xDefaultSet.atxTextures[2] = Utils.TryLoadTex(hatPath + "Down", manager);
            __result.xDefaultSet.atxTextures[3] = Utils.TryLoadTex(hatPath + "Left", manager);
            foreach (var kvp in __result.denxAlternateVisualSets)
            {
                string altPath = hatPath + modEquip.hatAltSetResources[kvp.Key] + "/";
                kvp.Value.atxTextures[0] = Utils.TryLoadTex(altPath + "Up", manager);
                kvp.Value.atxTextures[1] = Utils.TryLoadTex(altPath + "Right", manager);
                kvp.Value.atxTextures[2] = Utils.TryLoadTex(altPath + "Down", manager);
                kvp.Value.atxTextures[3] = Utils.TryLoadTex(altPath + "Left", manager);
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
                __instance.ditxWeaponTextures.Add(kvp.Key, Utils.TryLoadTex(kvp.Value.Replace("Weapons/", ""), __instance.contWeaponContent));

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

                var managerToUse = modItem ? ModLibrary.Global.ModItems[enType].equipInfo.managerToUse : VanillaContent;
                string path = $"Sprites/Heroes/{sAnimation}" + (modItem ? "" : "/Shields") + $"/{resource}/{sDirection}";

                __result.txShield = Utils.TryLoadTex(path, managerToUse);
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
                        newBank = new WaveBank(audioEngine, $"{entry.owner.CustomAssets.RootDirectory}/Sound/{modBank}.xwb");
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
                    newBank = new WaveBank(audioEngine, $"{entry.owner.CustomAssets.RootDirectory}/Sound/{modBank}.xwb");
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
            // This portion is copied from vanilla, with some file name modifications

            Game1 Game = GrindScript.Game;

            string sBackupDirectory = "";
            string sAppData = Game.sAppData;
            int carouselToUse = Game.xLocalPlayer.iSaveCarousel - 1;
            if (carouselToUse < 0)
                carouselToUse += 5;

            if (File.Exists(sAppData + "Characters/" + iFileSlot + ".mod.cha"))
            {
                if (Game.xLocalPlayer.sSaveableName == "")
                {
                    Game.xLocalPlayer.sSaveableName = Game.xLocalPlayer.sNetworkNickname;
                    string invalid = new string(Path.GetInvalidFileNameChars());
                    string text = invalid;
                    foreach (char c in text)
                    {
                        Game.xLocalPlayer.sSaveableName = Game.xLocalPlayer.sSaveableName.Replace(c.ToString(), "");
                    }
                }
                string sFolderPath = sAppData + "Backups/" + Game.xLocalPlayer.sSaveableName + "_" + Game.xLocalPlayer.xJournalInfo.iCollectorID + iFileSlot;
                if (!Directory.Exists(sFolderPath))
                {
                    Directory.CreateDirectory(sFolderPath);
                }
                sFolderPath += "/";
                sBackupDirectory = sFolderPath;
                uint iTimeSinceSave = Game.xLocalPlayer.iTimePlayed - Game.xLocalPlayer.iLastAutoSaveAt;
                if (iTimeSinceSave > 216000)
                {
                    int iHoursPlayed = (int)(Game.xLocalPlayer.iTimePlayed / 216000u);
                    string sHoursPlayed = iHoursPlayed.ToString();
                    while (sHoursPlayed.Length < 3)
                    {
                        sHoursPlayed = "0" + sHoursPlayed;
                    }
                    File.Copy(sAppData + "Characters/" + iFileSlot + ".mod.cha", sFolderPath + sHoursPlayed + "h.mod.cha", overwrite: true);
                    if (File.Exists(sAppData + "Worlds/" + iFileSlot + ".wld"))
                    {
                        File.Copy(sAppData + "Worlds/" + iFileSlot + ".mod.wld", sFolderPath + sHoursPlayed + "h.mod.wld", overwrite: true);
                    }
                    iHoursPlayed -= 10;
                    if (iHoursPlayed > 0)
                    {
                        sHoursPlayed = iHoursPlayed.ToString();
                        while (sHoursPlayed.Length < 3)
                        {
                            sHoursPlayed = "0" + sHoursPlayed;
                        }
                        if (File.Exists(sFolderPath + sHoursPlayed + "h.mod.cha"))
                        {
                            File.Delete(sFolderPath + sHoursPlayed + "h.mod.cha");
                        }
                        if (File.Exists(sFolderPath + sHoursPlayed + "h.mod.wld"))
                        {
                            File.Delete(sFolderPath + sHoursPlayed + "h.mod.wld");
                        }
                    }
                    Game.xLocalPlayer.iLastAutoSaveAt = Game.xLocalPlayer.iTimePlayed;
                }
                File.Copy(sAppData + "Characters/" + iFileSlot + ".mod.cha", sFolderPath + "auto" + carouselToUse + ".mod.cha", overwrite: true);
                if (File.Exists(sAppData + "Worlds/" + iFileSlot + ".wld"))
                {
                    File.Copy(sAppData + "Worlds/" + iFileSlot + ".mod.wld", sFolderPath + "auto" + carouselToUse + ".mod.wld", overwrite: true);
                }
            }

            FileStream writeStream = new FileStream(sAppData + "Characters/" + iFileSlot + ".mod.cha.temp", FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(writeStream);

            // Do the save
            GrindScript.Logger.Info($"Saving mod character {iFileSlot}...");
            ModSaveLoad.SaveModCharacter(bw);

            bw.Close();
            try
            {
                File.Copy(sAppData + "Characters/" + iFileSlot + ".mod.cha.temp", sAppData + "Characters/" + iFileSlot + ".mod.cha", overwrite: true);
                if (sBackupDirectory != "")
                {
                    File.Copy(sAppData + "Characters/" + iFileSlot + ".mod.cha.temp", sBackupDirectory + "latest.mod.cha", overwrite: true);
                }
                File.Delete(sAppData + "Characters/" + iFileSlot + ".mod.cha.temp");
            }
            catch { }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Saving_SaveWorldToFile(int)"/>.
        /// </summary>

        private static void PostWorldSave(int iFileSlot)
        {
            Game1 Game = GrindScript.Game;

            string sBackupDirectory = "";
            string sAppData = Game.sAppData;

            if (File.Exists(sAppData + "Characters/" + iFileSlot + ".mod.cha"))
            {
                if (Game.xLocalPlayer.sSaveableName == "")
                {
                    Game.xLocalPlayer.sSaveableName = Game.xLocalPlayer.sNetworkNickname;
                    string invalid = new string(Path.GetInvalidFileNameChars());
                    string text = invalid;
                    foreach (char c in text)
                    {
                        Game.xLocalPlayer.sSaveableName = Game.xLocalPlayer.sSaveableName.Replace(c.ToString(), "");
                    }
                }
                string sFolderPath = sAppData + "Backups/" + Game.xLocalPlayer.sSaveableName + "_" + Game.xLocalPlayer.xJournalInfo.iCollectorID + iFileSlot;
                if (!Directory.Exists(sFolderPath))
                {
                    Directory.CreateDirectory(sFolderPath);
                }
                sFolderPath += "/";
                sBackupDirectory = sFolderPath;
            }
            FileStream writeStream = new FileStream(sAppData + "Worlds/" + iFileSlot + ".mod.wld.temp", FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(writeStream);

            // Do the save
            GrindScript.Logger.Info($"Saving mod world {iFileSlot}...");
            ModSaveLoad.SaveModWorld(bw);

            bw.Close();
            try
            {
                File.Copy(sAppData + "Worlds/" + iFileSlot + ".mod.wld.temp", sAppData + "Worlds/" + iFileSlot + ".mod.wld", overwrite: true);
                if (sBackupDirectory != "" && iFileSlot != 100)
                {
                    File.Copy(sAppData + "Worlds/" + iFileSlot + ".mod.wld.temp", sBackupDirectory + "latest.mod.wld", overwrite: true);
                }
                File.Delete(sAppData + "Worlds/" + iFileSlot + ".mod.wld.temp");
            }
            catch { }
        }

        /// <summary>
        /// Runs after <see cref="Game1._Saving_SaveRogueToFile(string)"/>
        /// </summary>

        private static void PostArcadiaSave()
        {
            string sFileNameToUse = "arcademode.mod.sav";
            string sAppData = GrindScript.Game.sAppData;

            if (CAS.IsDebugFlagSet("OtherArcadeMode"))
            {
                sFileNameToUse = "arcademode_other.mod.sav";
            }
            FileStream writeStream = new FileStream(sAppData + sFileNameToUse + ".temp", FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(writeStream);

            // Do the save
            GrindScript.Logger.Info($"Saving mod arcade...");
            ModSaveLoad.SaveModArcade(bw);

            bw.Close();
            File.Copy(sAppData + sFileNameToUse + ".temp", sAppData + sFileNameToUse, overwrite: true);
            File.Delete(sAppData + sFileNameToUse + ".temp");
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadCharacterFromFile(int, bool)"/>
        /// </summary>

        private static void PostCharacterLoad(int iFileSlot, bool bAppearanceOnly)
        {
            string sAppData = GrindScript.Game.sAppData;
            if (!File.Exists(sAppData + "Characters/" + iFileSlot + ".mod.cha"))
            {
                return;
            }
            FileStream readStream = new FileStream(sAppData + "Characters/" + iFileSlot + ".mod.cha", FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(readStream);

            // Do the load
            GrindScript.Logger.Info($"Loading mod character {iFileSlot}...");
            ModSaveLoad.LoadModCharacter(br);

            br.Close();
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadWorldFromFile(int)"/>
        /// </summary>

        private static void PostWorldLoad(int iFileSlot)
        {
            string sAppData = GrindScript.Game.sAppData;
            if (!File.Exists(sAppData + "Worlds/" + iFileSlot + ".mod.wld"))
            {
                return;
            }
            FileStream readStream = new FileStream(sAppData + "Worlds/" + iFileSlot + ".mod.wld", FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(readStream);

            // Do the load
            GrindScript.Logger.Info($"Loading mod world {iFileSlot}...");
            ModSaveLoad.LoadModWorld(br);

            br.Close();
        }

        /// <summary>
        /// Runs after <see cref="Game1._Loading_LoadRogueFile(string)"/>
        /// </summary>

        private static void PostArcadiaLoad()
        {
            string sAppData = GrindScript.Game.sAppData;
            string sFileNameToUse = "arcademode.mod.sav";

            if (RogueLikeMode.LockedOutDueToHigherVersionSaveFile)
            {
                return;
            }
            if (CAS.IsDebugFlagSet("OtherArcadeMode"))
            {
                sFileNameToUse = "arcademode_other.mod.sav";
            }
            if (!File.Exists(sAppData + sFileNameToUse))
            {
                return;
            }
            FileStream readStream = new FileStream(sAppData + sFileNameToUse, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(readStream);

            // Do the load
            GrindScript.Logger.Info($"Loading mod arcade...");
            ModSaveLoad.LoadModArcade(br);

            br.Close();
        }
    }
}
