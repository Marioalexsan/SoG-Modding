using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using LevelLoading;
using SoG.Modding.Core;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using SoG.Modding.Tools;

namespace SoG.Modding.Patches
{
    internal static partial class PatchCollection
    {
        private static bool OnGetLevelBlueprint(ref LevelBlueprint __result, Level.ZoneEnum enZoneToGet)
        {
            if (!enZoneToGet.IsFromMod())
                return true;

            LevelBlueprint bprint = new LevelBlueprint();

            RareUtils.BlueprintSanityCheck(bprint);

            ModLevelEntry entry = ModGlobals.API.Library.Levels[enZoneToGet];

            try
            {
                entry.Builder?.Invoke(bprint);
            }
            catch (Exception e)
            {
                ModGlobals.Log.Error($"Builder threw an exception for level {enZoneToGet}! Exception: {e}");
                bprint = new LevelBlueprint();
            }

            RareUtils.BlueprintSanityCheck(bprint, true);

            // Enforce certain values

            bprint.enRegion = entry.Region;
            bprint.enZone = entry.GameID;
            bprint.sDefaultMusic = ""; // TODO Custom music
            bprint.sDialogueFiles = ""; // TODO Dialogue Files
            bprint.sMenuBackground = "bg01_mountainvillage"; // TODO Proper custom backgrounds. Transpiling _Level_Load is a good idea.
            bprint.sZoneName = ""; // TODO Zone titles


            // Loader setup

            Loader.afCurrentHeightLayers = new float[bprint.aiLayerDefaultHeight.Length];
            for (int i = 0; i < bprint.aiLayerDefaultHeight.Length; i++)
                Loader.afCurrentHeightLayers[i] = bprint.aiLayerDefaultHeight[i];

            Loader.lxCurrentSC = bprint.lxInvisibleWalls;

            // Return from method

            __result = bprint;
            return false;
        }

        private static void OnLevelLoadDoStuff(Level.ZoneEnum type, bool staticOnly)
        {
            // Modifying vanilla levels not supported yet

            if (!type.IsFromMod()) return;

            ModLevelEntry entry = ModGlobals.API.Library.Levels[type];

            try
            {
                entry.Loader?.Invoke(staticOnly);
            }
            catch (Exception e)
            {
                ModGlobals.Log.Error($"Loader threw an exception for level {type}! Exception: {e}");
            }
        }
    }
}
