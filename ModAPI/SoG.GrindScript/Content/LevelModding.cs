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

        public static Level.ZoneEnum CreateLevel(BaseScript owner, LevelConfig config)
        {
            if (config == null || owner == null)
            {
                GrindScript.Logger.Warn("Owner or config is null!");
                return Level.ZoneEnum.None;
            }

            Level.ZoneEnum allocatedType = IDAllocator.NewZoneEnum();

            ModLevel baseEntry = config.CreateLevel(allocatedType);

            ModLibrary.Levels[allocatedType] = new ModLevelEntry()
            {
                owner = owner,
                type = allocatedType,
                levelInfo = baseEntry
            };

            return allocatedType;
        }
    }
}
