using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SoG.Modding
{
    public class LevelModding
    {
        public static Level.WorldRegion CreateWorldRegion()
        {
            var VanillaContent = GrindScript.Game.Content;

            var allocatedType = IDAllocator.NewWorldRegion();

            GrindScript.Game.xLevelMaster.denxRegionContent.Add(allocatedType, new ContentManager(VanillaContent.ServiceProvider, VanillaContent.RootDirectory));

            return allocatedType;
        }

        public static Level.ZoneEnum CreateLevel(BaseScript owner, LevelConfig cfg)
        {
            if (cfg == null || owner == null)
            {
                GrindScript.Logger.Warn("Owner or config is null!");
                return Level.ZoneEnum.None;
            }

            Level.ZoneEnum gameID = IDAllocator.NewZoneEnum();

            ModLibrary.Levels[gameID] = new ModLevelEntry(owner, gameID)
            {
                Builder = cfg.Builder,
                Loader = cfg.Loader,
                Region = cfg.WorldRegion
            };

            return gameID;
        }
    }
}
