using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Utils;

namespace SoG.Modding.Core
{
    // Note: Use of this static class should be limited (when possible) to:
    // - PatchCollection methods (or patches in general)
    // - Extension methods for SoG
    // - Methods in the Utils namespace (if using a class instance doesn't make sense)

    /// <summary>
    /// Provides a common access point to default objects.
    /// </summary>
    public static class APIGlobals
    {
        /// <summary>
        /// The default GrindScript instance.
        /// </summary>
        public static GrindScript API { get; internal set; }

        /// <summary>
        /// The current Game1 instance. Also known as Secrets of Grindea.
        /// </summary>
        public static Game1 Game { get; internal set; }

        /// <summary>
        /// The default Console Logger.
        /// </summary>
        public static ConsoleLogger Logger { get; internal set; }
    }
}
