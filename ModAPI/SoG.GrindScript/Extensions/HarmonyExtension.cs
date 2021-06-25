using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public static class HarmonyExtension
    {
        /// <summary>
        /// Creates patches by specifying a PatchDescription.
        /// </summary>

        public static MethodInfo Patch(this Harmony harmony, PatchCodex.PatchInfo patch)
        {
            if (patch.Target == null || (patch.Prefix == null && patch.Postfix == null && patch.Transpiler == null))
            {
                GrindScript.Logger.Warn("A patch provided is likely invalid!");
            }

            return harmony.Patch(patch.Target,
                patch.Prefix != null ? new HarmonyMethod(patch.Prefix) : null,
                patch.Postfix != null ? new HarmonyMethod(patch.Postfix) : null,
                patch.Transpiler != null ? new HarmonyMethod(patch.Transpiler) : null,
                null);
        }
    }
}
