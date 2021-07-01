using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace SoG.Modding
{
    public static class RoguelikeModding
    {
        public static RogueLikeMode.TreatsCurses CreateTreatOrCurse(BaseScript owner, TreatCurseConfig cfg)
        {
            if (cfg == null || owner == null)
            {
                GrindScript.Logger.Error("Can't create treat or curse because owner or cfg is null.");
                return RogueLikeMode.TreatsCurses.None;
            }

            RogueLikeMode.TreatsCurses allocatedType = IDAllocator.NewTreatOrCurse();

            ModCurseEntry entry = new ModCurseEntry()
            {
                Owner = owner,
                ModID = cfg.UniqueID,
                GameID = allocatedType,
                isTreat = cfg.IsTreat,
                nameHandle = "TreatCurse_" + (int)allocatedType + "_Name",
                descriptionHandle = "TreatCurse_" + (int)allocatedType + "_Description",
                resourcePath = cfg.TexturePath,
                scoreModifier = cfg.ScoreModifier
            };

            ModLibrary.GlobalLib.TreatsCurses[allocatedType] = entry;
            owner.ModLib.TreatsCurses[allocatedType] = entry;

            if (owner.ModLib.TreatsCurses.Values.Any(x => x != entry && x.ModID == cfg.UniqueID))
            {
                GrindScript.Logger.Error($"Mod {owner.GetType().Name} has two or more treats / curses with the uniqueID {cfg.UniqueID}!");
                GrindScript.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            TextModding.AddMiscText("Menus", entry.nameHandle, cfg.Name, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Menus", entry.descriptionHandle, cfg.Description, MiscTextTypes.GenericItemDescription);

            return allocatedType;
        }

        public static RogueLikeMode.Perks CreatePerk(BaseScript owner, PerkConfig cfg)
        {
            if (cfg == null || owner == null)
            {
                GrindScript.Logger.Error("Can't create perk because owner or cfg is null.");
                return RogueLikeMode.Perks.None;
            }

            RogueLikeMode.Perks allocatedType = IDAllocator.NewRoguelikePerk();

            string textEntry = ((int)allocatedType).ToString();

            ModPerkEntry entry = new ModPerkEntry()
            {
                Owner = owner,
                ModID = cfg.UniqueID,
                GameID = allocatedType,
                resourcePath = cfg.TexturePath,
                activator = cfg.RunStartActivator,
                essenceCost = cfg.EssenceCost,
                textEntry = textEntry
            };

            ModLibrary.GlobalLib.Perks[allocatedType] = entry;
            owner.ModLib.Perks[allocatedType] = entry;

            if (owner.ModLib.Perks.Values.Any(x => x != entry && x.ModID == cfg.UniqueID))
            {
                GrindScript.Logger.Error($"Mod {owner.GetType().Name} has two or more treats / curses with the uniqueID {cfg.UniqueID}!");
                GrindScript.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            TextModding.AddMiscText("Menus", "Perks_Name_" + textEntry, cfg.Name, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Menus", "Perks_Description_" + textEntry, cfg.Description, MiscTextTypes.GenericItemDescription);

            return allocatedType;
        }
    }
}
