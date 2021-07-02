using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Tools;

namespace SoG.Modding.Core
{
    // Note: Only static methods can act as Harmony Patches
    // Therefore, this static class must be used to supply state to patches

    /// <summary>
    /// Provides a common access point to default objects.
    /// </summary>

    public static class ModGlobals
    {
        /// <summary> The GrindScript instance. Use methods from this class to add modded content. </summary>
        public static GrindScript API { get; internal set; }

        /// <summary> The Game1 instance. Also known as Secrets of Grindea. </summary>
        public static Game1 Game { get; internal set; }

        /// <summary> Default Console Logger. Logging too many things usually turns it into a CLogger, heh. </summary>
        public static ConsoleLogger Log { get; internal set; }
    }
}
