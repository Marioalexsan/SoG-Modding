using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public static class RoguelikeExtension
    {
        /// <summary>
        /// Checks if the given item ID is a vanilla item (i.e. is present in the base game)
        /// </summary>

        public static bool IsSoGTreatCurse(this RogueLikeMode.TreatsCurses enType)
        {
            return Enum.IsDefined(typeof(RogueLikeMode.TreatsCurses), enType);
        }

        /// <summary>
        /// Checks if the given item ID is a mod item (i.e. it is currently allocated by GrindScript)
        /// </summary>

        public static bool IsModTreatCurse(this RogueLikeMode.TreatsCurses enType)
        {
            return enType >= IDAllocator.TreatsCursesStart && enType < IDAllocator.TreatsCursesEnd;
        }

        /// <summary>
        /// Checks if the given item ID is a vanilla item (i.e. is present in the base game)
        /// </summary>

        public static bool IsSoGPerk(this RogueLikeMode.Perks enType)
        {
            return Enum.IsDefined(typeof(RogueLikeMode.Perks), enType);
        }

        /// <summary>
        /// Checks if the given item ID is a mod item (i.e. it is currently allocated by GrindScript)
        /// </summary>

        public static bool IsModPerk(this RogueLikeMode.Perks enType)
        {
            return enType >= IDAllocator.RoguelikePerkStart && enType < IDAllocator.RoguelikePerkEnd;
        }
    }
}
