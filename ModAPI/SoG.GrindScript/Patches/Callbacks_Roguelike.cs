using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Core;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;

namespace SoG.Modding.Patches
{
    internal static partial class PatchCollection
    {
        private static bool OnGetPerkTexture(RogueLikeMode.Perks enPerk, ref Texture2D __result)
        {
            if (!enPerk.IsFromMod())
                return true;

            string path = APIGlobals.API.Library.Perks[enPerk].ResourcePath;

            __result = path != "" ? Tools.TryLoadTex(path, APIGlobals.Game.Content) : APIGlobals.API.MissingTex;

            return false;
        }

        private static bool OnGetTreatCurseTexture(RogueLikeMode.TreatsCurses enTreat, ref Texture2D __result)
        {
            if (!enTreat.IsFromMod())
                return true;

            string path = APIGlobals.API.Library.TreatsCurses[enTreat].ResourcePath;

            __result = path != "" ? Tools.TryLoadTex(path, APIGlobals.Game.Content) : APIGlobals.API.MissingTex;

            return false;
        }

        private static bool OnGetTreatCurseInfo(RogueLikeMode.TreatsCurses enTreatCurse, out string sNameHandle, out string sDescriptionHandle, out float fScoreModifier)
        {
            if (!enTreatCurse.IsFromMod())
            {
                sNameHandle = "";
                sDescriptionHandle = "";
                fScoreModifier = 1f;
                return true;
            }

            var entry = APIGlobals.API.Library.TreatsCurses[enTreatCurse];

            sNameHandle = entry.NameHandle;
            sDescriptionHandle = entry.DescriptionHandle;
            fScoreModifier = entry.ScoreModifier;

            return false;
        }

        private static void PostFillCurseList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in APIGlobals.API.Library.TreatsCurses)
            {
                if (!kvp.Value.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        private static void PostFillTreatList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in APIGlobals.API.Library.TreatsCurses)
            {
                if (kvp.Value.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        private static void PostPerkActivation(PlayerView xView, List<RogueLikeMode.Perks> len)
        {
            foreach (var perk in len)
            {
                if (perk.IsFromMod())
                    APIGlobals.API.Library.Perks[perk].Activator?.Invoke(xView);
            }
        }

        private static void PostPerkListInit()
        {
            APIGlobals.Logger.Debug("Init!");
            foreach (var perk in APIGlobals.API.Library.Perks.Values)
                RogueLikeMode.PerkInfo.lxAllPerks.Add(new RogueLikeMode.PerkInfo(perk.GameID, perk.EssenceCost, perk.TextEntry));
        }
    }
}
