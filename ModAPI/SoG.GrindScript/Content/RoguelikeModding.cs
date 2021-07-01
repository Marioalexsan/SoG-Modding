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

            RogueLikeMode.TreatsCurses gameID = IDAllocator.NewTreatOrCurse();

            ModCurseEntry entry = new ModCurseEntry(owner, gameID, cfg.ModID)
            {
                IsTreat = cfg.IsTreat,
                NameHandle = "TreatCurse_" + (int)gameID + "_Name",
                DescriptionHandle = "TreatCurse_" + (int)gameID + "_Description",
                ResourcePath = cfg.TexturePath,
                ScoreModifier = cfg.ScoreModifier
            };

            ModLibrary.GlobalLib.TreatsCurses[gameID] = owner.ModLib.TreatsCurses[gameID] = entry;

            if (owner.ModLib.TreatsCurses.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                GrindScript.Logger.Error($"Mod {owner.GetType().Name} has two or more treats / curses with the uniqueID {cfg.ModID}!");
                GrindScript.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            TextModding.AddMiscText("Menus", entry.NameHandle, cfg.Name, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Menus", entry.DescriptionHandle, cfg.Description, MiscTextTypes.GenericItemDescription);

            return gameID;
        }

        public static RogueLikeMode.Perks CreatePerk(BaseScript owner, PerkConfig cfg)
        {
            if (cfg == null || owner == null)
            {
                GrindScript.Logger.Error("Can't create perk because owner or cfg is null.");
                return RogueLikeMode.Perks.None;
            }

            RogueLikeMode.Perks gameID = IDAllocator.NewRoguelikePerk();

            ModPerkEntry entry = new ModPerkEntry(owner, gameID, cfg.ModID)
            {
                ResourcePath = cfg.TexturePath,
                Activator = cfg.RunStartActivator,
                EssenceCost = cfg.EssenceCost,
                TextEntry = ((int)gameID).ToString()
            };

            ModLibrary.GlobalLib.Perks[gameID] = entry;
            owner.ModLib.Perks[gameID] = entry;

            if (owner.ModLib.Perks.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                GrindScript.Logger.Error($"Mod {owner.GetType().Name} has two or more treats / curses with the uniqueID {cfg.ModID}!");
                GrindScript.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            TextModding.AddMiscText("Menus", "Perks_Name_" + entry.TextEntry, cfg.Name, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Menus", "Perks_Description_" + entry.TextEntry, cfg.Description, MiscTextTypes.GenericItemDescription);

            return gameID;
        }
    }
}
