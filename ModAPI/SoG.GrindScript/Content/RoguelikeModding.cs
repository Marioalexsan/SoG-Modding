using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Core;
using System.Diagnostics;
using SoG.Modding.Utils;
using SoG.Modding.Content.Configs;

namespace SoG.Modding.Content
{
    public class RoguelikeModding : ModdingLogic
    {
        public RoguelikeModding(GrindScript modAPI)
            : base(modAPI) { }

        public RogueLikeMode.TreatsCurses CreateTreatOrCurse(TreatCurseConfig cfg)
        {
            ThrowHelper.ThrowIfNull(cfg);

            BaseScript mod = _modAPI.CurrentModContext;
            if (mod == null)
            {
                _modAPI.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateTreatOrCurse));
                return RogueLikeMode.TreatsCurses.None;
            }


            RogueLikeMode.TreatsCurses gameID = _modAPI.Allocator.TreatCurseID.Allocate();

            ModCurseEntry entry = new ModCurseEntry(mod, gameID, cfg.ModID)
            {
                IsTreat = cfg.IsTreat,
                NameHandle = "TreatCurse_" + (int)gameID + "_Name",
                DescriptionHandle = "TreatCurse_" + (int)gameID + "_Description",
                ResourcePath = cfg.TexturePath,
                ScoreModifier = cfg.ScoreModifier
            };

            _modAPI.Library.TreatsCurses[gameID] = mod.Library.TreatsCurses[gameID] = entry;

            if (mod.Library.TreatsCurses.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                _modAPI.Logger.Error($"Mod {mod.GetType().Name} has two or more treats / curses with the uniqueID {cfg.ModID}!");
                _modAPI.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            _modAPI.TextAPI.AddMiscText("Menus", entry.NameHandle, cfg.Name, MiscTextTypes.GenericItemName);
            _modAPI.TextAPI.AddMiscText("Menus", entry.DescriptionHandle, cfg.Description, MiscTextTypes.GenericItemDescription);

            return gameID;
        }

        public RogueLikeMode.Perks CreatePerk(PerkConfig cfg)
        {
            ThrowHelper.ThrowIfNull(cfg);

            BaseScript mod = _modAPI.CurrentModContext;
            if (mod == null)
            {
                _modAPI.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreatePerk));
                return RogueLikeMode.Perks.None;
            }

            RogueLikeMode.Perks gameID = _modAPI.Allocator.PerkID.Allocate();

            ModPerkEntry entry = new ModPerkEntry(mod, gameID, cfg.ModID)
            {
                ResourcePath = cfg.TexturePath,
                Activator = cfg.RunStartActivator,
                EssenceCost = cfg.EssenceCost,
                TextEntry = ((int)gameID).ToString()
            };

            _modAPI.Library.Perks[gameID] = entry;
            mod.Library.Perks[gameID] = entry;

            if (mod.Library.Perks.Values.Any(x => x != entry && x.ModID == cfg.ModID))
            {
                _modAPI.Logger.Error($"Mod {mod.GetType().Name} has two or more treats / curses with the uniqueID {cfg.ModID}!");
                _modAPI.Logger.Error($"This is likely to break loading, saving, and many other things!");
            }

            _modAPI.TextAPI.AddMiscText("Menus", "Perks_Name_" + entry.TextEntry, cfg.Name, MiscTextTypes.GenericItemName);
            _modAPI.TextAPI.AddMiscText("Menus", "Perks_Description_" + entry.TextEntry, cfg.Description, MiscTextTypes.GenericItemDescription);

            return gameID;
        }
    }
}
