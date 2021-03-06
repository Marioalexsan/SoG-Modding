﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using SoG.Modding.Core;
using SoG.Modding.Utils;
using SoG.Modding.Extensions;

namespace SoG.Modding.Patches
{
    internal static partial class PatchCollection
    {
        private static void OnGame1Initialize()
        {
            APIGlobals.API.SetupSoG();
        }

        private static bool OnChatParseCommand(string command, string message, int connection)
        {
            string[] words = command.Split(':');
            if (words.Length < 2)
                return false; // Is probably a vanilla command

            string target = words[0];
            string trueCommand = command.Substring(command.IndexOf(':') + 1);

            if (!APIGlobals.API.Library.Commands.TryGetValue(target, out var parsers))
            {
                CAS.AddChatMessage($"[{GrindScriptCommands.APIName}] Unknown mod!");
                return true;
            }
            if (!parsers.TryGetValue(trueCommand, out var parser))
            {
                if (trueCommand == "Help")
                {
                    OnChatParseCommand($"{GrindScriptCommands.APIName}:Help", target, connection);
                    return true;
                }

                CAS.AddChatMessage($"[{target}] Unknown command!");
                return true;
            }

            APIGlobals.Logger.Debug($"Parsed command {target} : {trueCommand}, argument list: {message}");
            parser(message, connection);

            return true;
        }

        /// <summary>
        /// Patches <see cref="Game1._Animations_GetAnimationSet(PlayerView, string, string, bool, bool, bool, bool)"/>.
        /// Allows use of modded assets. Modded shields have modified asset paths compared to Vanilla.
        /// </summary>
        private static bool OnGetAnimationSet(PlayerView xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bCustomHat, bool bWithShield, bool bWeaponOnTop, ref PlayerAnimationTextureSet __result)
        {
            ContentManager VanillaContent = RenderMaster.contPlayerStuff;

            __result = new PlayerAnimationTextureSet() 
            { 
                bWeaponOnTop = bWeaponOnTop,
                txBase = Utils.Tools.TryLoadTex($"Sprites/Heroes/{sAnimation}/{sDirection}", VanillaContent)
            };

            string resource = xPlayerView.xEquipment.DisplayShield?.sResourceName ?? "";
            if (bWithShield && resource != "")
            {
                var enType = xPlayerView.xEquipment.DisplayShield.enItemType;
                bool modItem = enType.IsFromMod();

                if (modItem)
                {
                    __result.txShield = Utils.Tools.TryLoadTex($"{resource}/{sAnimation}/{sDirection}", APIGlobals.API.Library.Items[enType].Manager);
                }
                else
                {
                    __result.txShield = Utils.Tools.TryLoadTex($"Sprites/Heroes/{sAnimation}/Shields/{resource}/{sDirection}", VanillaContent);
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
            var redirects = APIGlobals.API.Library.VanillaMusicRedirects;
            string audioIDToUse = sSongName;

            if (!audioIDToUse.StartsWith("GS_") && redirects.ContainsKey(audioIDToUse))
                audioIDToUse = redirects[audioIDToUse];

            sSongName = audioIDToUse;
        }

        /// <summary>
        /// Replaces method <see cref="SoundSystem.ChangeSongRegionIfNecessary"/>.
        /// Allows using music from modded sources.
        /// </summary>
        private static bool OnChangeSongRegionIfNecessary(ref SoundSystem __instance, string sSongName)
        {
            // This will probably cause you brain and eye damage if you read it

            var audioAPI = APIGlobals.API.AudioAPI;

            SoundSystem soundSystem = __instance;
            TypeInfo soundType = typeof(SoundSystem).GetTypeInfo();

            BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Instance;

            var f_musicWaveBank = soundType.GetField("musicWaveBank", flag);
            var f_loadedMusicWaveBank = soundType.GetField("loadedMusicWaveBank", flag);

            var dsxStandbyWaveBanks = soundType.GetField("dsxStandbyWaveBanks", flag).GetValue(soundSystem) as Dictionary<string, WaveBank>;
            var dssSongRegionMap = soundType.GetField("dssSongRegionMap", flag).GetValue(soundSystem) as Dictionary<string, string>;
            var universalMusic = soundType.GetField("universalMusicWaveBank", flag).GetValue(soundSystem) as WaveBank;
            var audioEngine = soundType.GetField("audioEngine", flag).GetValue(soundSystem) as AudioEngine;

            bool currentIsModded = Utils.Tools.SplitGSAudioID(sSongName, out int entryID, out bool isMusic, out int cueID);

            if (currentIsModded && !isMusic)
                APIGlobals.Logger.Warn($"Trying to play modded audio as music, but the audio isn't music! ID: {sSongName}");

            ModAudioEntry entry = currentIsModded ? APIGlobals.API.Library.Audio[entryID] : null;
            string cueName = currentIsModded ? entry.MusicNames[cueID] : sSongName;
            string nextBankName = currentIsModded ? entry.MusicBankNames[cueName] : dssSongRegionMap[sSongName];

            WaveBank currentMusicBank = f_musicWaveBank.GetValue(soundSystem) as WaveBank;

            if (audioAPI.IsUniversalMusicBank(nextBankName))
            {
                if (currentIsModded && entry.UniversalWB == null)
                {
                    APIGlobals.Logger.Error($"{sSongName} requested modded UniversalMusic bank, but the bank does not exist!");
                    return false;
                }

                if (currentMusicBank != null && !audioAPI.IsUniversalMusicBank(currentMusicBank))
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);

                f_musicWaveBank.SetValue(soundSystem, currentIsModded ? entry.UniversalWB : universalMusic);
            }
            else if (soundSystem.sCurrentMusicWaveBank != nextBankName)
            {
                if (currentMusicBank != null && !audioAPI.IsUniversalMusicBank(currentMusicBank) && !currentMusicBank.IsDisposed)
                    soundSystem.SetStandbyBank(soundSystem.sCurrentMusicWaveBank, currentMusicBank);

                soundSystem.sCurrentMusicWaveBank = nextBankName;

                if (dsxStandbyWaveBanks.ContainsKey(nextBankName))
                {
                    f_musicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[nextBankName]);
                    dsxStandbyWaveBanks.Remove(nextBankName);
                }
                else
                {
                    string root = Path.Combine(APIGlobals.Game.Content.RootDirectory, currentIsModded ? entry.Owner.ModPath : "");

                    f_loadedMusicWaveBank.SetValue(soundSystem, new WaveBank(audioEngine, Path.Combine(root, "Sound", $"{nextBankName}.xwb")));
                    f_musicWaveBank.SetValue(soundSystem, null);
                }
                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
                soundType.GetPrivateInstanceMethod("CheckStandbyBanks").Invoke(soundSystem, new object[] { nextBankName });
            }
            else if (audioAPI.IsUniversalMusicBank(currentMusicBank))
            {
                if (dsxStandbyWaveBanks.ContainsKey(soundSystem.sCurrentMusicWaveBank))
                {
                    f_musicWaveBank.SetValue(soundSystem, dsxStandbyWaveBanks[soundSystem.sCurrentMusicWaveBank]);
                    dsxStandbyWaveBanks.Remove(soundSystem.sCurrentMusicWaveBank);
                    return false;
                }

                string root = Path.Combine(APIGlobals.Game.Content.RootDirectory, currentIsModded ? entry.Owner.ModPath : "");
                string bankToUse = currentIsModded ? nextBankName : soundSystem.sCurrentMusicWaveBank;

                f_loadedMusicWaveBank.SetValue(soundSystem, new WaveBank(audioEngine, Path.Combine(root, "Sound", bankToUse + ".xwb")));
                f_musicWaveBank.SetValue(soundSystem, null);

                soundSystem.xMusicVolumeMods.iMusicCueRetries = 0;
                soundSystem.xMusicVolumeMods.sSongInWait = sSongName;
            }

            return false; // Never returns control to original
        }
    }
}