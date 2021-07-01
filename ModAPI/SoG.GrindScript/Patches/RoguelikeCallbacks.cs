using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.Modding
{
    internal static partial class Patches
    {
        private static bool OnGetPerkTexture(RogueLikeMode.Perks enPerk, ref Texture2D __result)
        {
            if (!enPerk.IsModPerk())
                return true;

            __result = Utils.TryLoadTex(ModLibrary.GlobalLib.Perks[enPerk].ResourcePath, GrindScript.Game.Content);

            return false;
        }

        private static bool OnGetTreatCurseTexture(RogueLikeMode.TreatsCurses enTreat, ref Texture2D __result)
        {
            if (!enTreat.IsModTreatCurse())
                return true;

            __result = Utils.TryLoadTex(ModLibrary.GlobalLib.TreatsCurses[enTreat].ResourcePath, GrindScript.Game.Content);

            return false;
        }

        private static bool OnGetTreatCurseInfo(RogueLikeMode.TreatsCurses enTreatCurse, out string sNameHandle, out string sDescriptionHandle, out float fScoreModifier)
        {
            if (!enTreatCurse.IsModTreatCurse())
            {
                sNameHandle = "";
                sDescriptionHandle = "";
                fScoreModifier = 1f;
                return true;
            }

            var entry = ModLibrary.GlobalLib.TreatsCurses[enTreatCurse];

            sNameHandle = entry.NameHandle;
            sDescriptionHandle = entry.DescriptionHandle;
            fScoreModifier = entry.ScoreModifier;

            return false;
        }

        private static void PostFillCurseList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in ModLibrary.GlobalLib.TreatsCurses)
            {
                if (!kvp.Value.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        private static void PostFillTreatList(ShopMenu.TreatCurseMenu __instance)
        {
            foreach (var kvp in ModLibrary.GlobalLib.TreatsCurses)
            {
                if (kvp.Value.IsTreat)
                    __instance.lenTreatCursesAvailable.Add(kvp.Value.GameID);
            }
        }

        private static void PostPerkActivation(PlayerView xView, List<RogueLikeMode.Perks> len)
        {
            foreach (var perk in len)
            {
                if (perk.IsModPerk())
                    ModLibrary.GlobalLib.Perks[perk].Activator?.Invoke(xView);
            }
        }

        private static void PostPerkListInit()
        {
            GrindScript.Logger.Debug("Init!");
            foreach (var perk in ModLibrary.GlobalLib.Perks.Values)
                RogueLikeMode.PerkInfo.lxAllPerks.Add(new RogueLikeMode.PerkInfo(perk.GameID, perk.EssenceCost, perk.TextEntry));
        }
    }
}
