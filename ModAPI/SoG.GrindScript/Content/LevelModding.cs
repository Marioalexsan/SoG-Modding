using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoG.Modding.Core;
using System.Diagnostics;
using SoG.Modding.Utils;
using SoG.Modding.Content.Configs;

namespace SoG.Modding.Content
{
    public class LevelModding : ModdingLogic
    {
        public LevelModding(GrindScript modAPI)
            : base(modAPI) { }

        public Level.WorldRegion CreateWorldRegion()
        {
            var VanillaContent = _modAPI.Game.Content;

            var gameID = _modAPI.Allocator.WorldRegionID.Allocate();

            _modAPI.Game.xLevelMaster.denxRegionContent.Add(gameID, new ContentManager(VanillaContent.ServiceProvider, VanillaContent.RootDirectory));

            return gameID;
        }

        /// <summary>
        /// Creates a new level from the given LevlConfig.
        /// Config must not be null.
        /// </summary>
        public Level.ZoneEnum CreateLevel(LevelConfig cfg)
        {
            ThrowHelper.ThrowIfNull(cfg);

            BaseScript mod = _modAPI.CurrentModContext;
            if (mod == null)
            {
                _modAPI.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateLevel));
                return Level.ZoneEnum.None;
            }

            Level.ZoneEnum gameID = _modAPI.Allocator.LevelID.Allocate();

            _modAPI.Library.Levels[gameID] = new ModLevelEntry(mod, gameID)
            {
                Builder = cfg.Builder,
                Loader = cfg.Loader,
                Region = cfg.WorldRegion
            };

            return gameID;
        }
    }
}
