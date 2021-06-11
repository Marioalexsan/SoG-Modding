using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public static class PatchHelper
    {
        public static IEnumerable<CodeInstruction> InsertAfterFirstMethod(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodInfo target, IEnumerable<CodeInstruction> codeToInsert)
        {
            var noReturnValue = target.ReturnType == typeof(void);
            int stage = 0;
            foreach (CodeInstruction ins in instructions)
            {
                if (stage == 0 && ins.Calls(target))
                {
                    stage = 1;
                    yield return ins;
                    continue;
                }
                if (stage == 1)
                {
                    if (noReturnValue || ins.opcode == OpCodes.Pop)
                    {
                        stage = 2;
                        yield return ins;
                        continue;
                    }
                    else throw new Exception("Transpile failed: insert target has return value that is being used!");
                }
                if (stage == 2)
                {
                    stage = 3;
                    foreach (CodeInstruction newIns in codeToInsert)
                        yield return newIns;
                }
                yield return ins;
            }
            if (stage != 3) throw new Exception("Transpile failed: couldn't find target!");
        }
    }

    public static partial class TypeExtension
    {
        public static MethodInfo Patch(this Harmony harmony, PatchCodex.PatchDescription patch)
        {
            if (patch.Target == null || (patch.Prefix == null && patch.Postfix == null && patch.Transpiler == null))
                Console.WriteLine("A patch provided is invalid!");
            return harmony.Patch(patch.Target,
                patch.Prefix != null ? new HarmonyMethod(patch.Prefix) : null,
                patch.Postfix != null ? new HarmonyMethod(patch.Postfix) : null,
                patch.Transpiler != null ? new HarmonyMethod(patch.Transpiler) : null,
                null);
        }
    }
}
