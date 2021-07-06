using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Utils
{
    /// <summary>
    /// Provides methods for common throw logic.
    /// </summary>
    public static class ThrowHelper
    {
        /// <summary>
        /// Throws ArgumentNullException if any of the arguments in args are null.
        /// </summary>
        public static void ThrowIfNull(params object[] args)
        {
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new ArgumentNullException("One or more arguments are null.");
            }
        }
    }
}
