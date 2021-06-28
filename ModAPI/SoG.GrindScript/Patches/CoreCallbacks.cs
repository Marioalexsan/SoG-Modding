using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

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

            if (!ModLibrary.Global.Commands.TryGetValue(target, out var parsers))
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
                    var managerToUse = ModLibrary.Global.Items[enType].equipInfo.managerToUse;
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

            ModAudioEntry entry = currentIsModded ? ModLibrary.Global.Audio[entryID] : null;
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
    }
}