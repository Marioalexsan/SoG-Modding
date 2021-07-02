using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    public class RoguelikeModding : ModdingLogic
    {
        public RoguelikeModding(GrindScript modAPI)
            : base(modAPI) { }

        public RogueLikeMode.TreatsCurses CreateTreatOrCurse(TreatCurseConfig cfg)
        {
            BaseScript owner = _modAPI.CurrentModContext;
            if (cfg == null || owner == null)
            {
                ModGlobals.Log.Error("Can't create treat or curse because owner or cfg is null.");
                return RogueLikeMode.TreatsCurses.None;
            }

            RogueLikeMode.TreatsCurses gameID = _modAPI.Allocator.TreatCurseID.Allocate();

            ModCurseEntry entry = new ModCurseEntry(owner, gameID, cfg.ModID)
            {
                IsTreat = cfg.IsTreat,
                NameHandle = "TreatCurse_" + (int)gameID + "_Name",
                DescriptionHandle = "TreatCurse_" + (int)gameID + "_Description",
                ResourcePath = cfg.TexturePath,
                ScoreModifier = cfg.ScoreModifier
            };

            _modAPI.Library.TreatsCurses[gameID] = owner.Library.TreatsCurses[gameID] = entry;

            if (owner.Library.TreatsCurses.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                ModGlobals.Log.Error($"Mod {owner.GetType().Name} has two or more treats / curses with the uniqueID {cfg.ModID}!");
                ModGlobals.Log.Error($"This is likely to break loading, saving, and many other things!");
            }

            TextModding.AddMiscText("Menus", entry.NameHandle, cfg.Name, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Menus", entry.DescriptionHandle, cfg.Description, MiscTextTypes.GenericItemDescription);

            return gameID;
        }

        public RogueLikeMode.Perks CreatePerk(PerkConfig cfg)
        {
            BaseScript owner = _modAPI.CurrentModContext;
            if (cfg == null || owner == null)
            {
                ModGlobals.Log.Error("Can't create perk because owner or cfg is null.");
                return RogueLikeMode.Perks.None;
            }

            RogueLikeMode.Perks gameID = _modAPI.Allocator.PerkID.Allocate();

            ModPerkEntry entry = new ModPerkEntry(owner, gameID, cfg.ModID)
            {
                ResourcePath = cfg.TexturePath,
                Activator = cfg.RunStartActivator,
                EssenceCost = cfg.EssenceCost,
                TextEntry = ((int)gameID).ToString()
            };

            _modAPI.Library.Perks[gameID] = entry;
            owner.Library.Perks[gameID] = entry;

            if (owner.Library.Perks.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                ModGlobals.Log.Error($"Mod {owner.GetType().Name} has two or more treats / curses with the uniqueID {cfg.ModID}!");
                ModGlobals.Log.Error($"This is likely to break loading, saving, and many other things!");
            }

            TextModding.AddMiscText("Menus", "Perks_Name_" + entry.TextEntry, cfg.Name, MiscTextTypes.GenericItemName);
            TextModding.AddMiscText("Menus", "Perks_Description_" + entry.TextEntry, cfg.Description, MiscTextTypes.GenericItemDescription);

            return gameID;
        }
    }
}
