using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    public class LevelModding : ModdingLogic
    {
        public LevelModding(GrindScript modAPI)
            : base(modAPI) { }

        public Level.WorldRegion CreateWorldRegion()
        {
            var VanillaContent = ModGlobals.Game.Content;

            var gameID = _modAPI.Allocator.WorldRegionID.Allocate();

            ModGlobals.Game.xLevelMaster.denxRegionContent.Add(gameID, new ContentManager(VanillaContent.ServiceProvider, VanillaContent.RootDirectory));

            return gameID;
        }

        public Level.ZoneEnum CreateLevel(LevelConfig cfg)
        {
            BaseScript owner = _modAPI.CurrentModContext;
            if (cfg == null || owner == null)
            {
                ModGlobals.Log.Warn("Owner or config is null!");
                return Level.ZoneEnum.None;
            }

            Level.ZoneEnum gameID = _modAPI.Allocator.LevelID.Allocate();

            _modAPI.Library.Levels[gameID] = new ModLevelEntry(owner, gameID)
            {
                Builder = cfg.Builder,
                Loader = cfg.Loader,
                Region = cfg.WorldRegion
            };

            return gameID;
        }
    }
}
