using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SoG.Modding
{
    public static class PatchHelper
    {
        /// <summary>
        /// <para> Transpiles the given instruction set by inserting code instructions after the first occurence of the target method. </para>
        /// <para> The target method must either have no return value, or a return value that is not used by the original code. </para>
        /// </summary>
        /// <returns> The modified code, with new instructions inserted as described. </returns>
        /// <exception cref="Exception"> Thrown if the transpile fails due to the described incompatibilities. </exception>
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
                        if (ins.opcode == OpCodes.Pop)
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
        /// <summary>
        /// Creates patches by specifying a PatchDescription.
        /// </summary>
        /// <returns> The replacement method that was created to patch the original method.</returns>
        public static MethodInfo Patch(this Harmony harmony, PatchCodex.PatchDescription patch)
        {
            if (patch.Target == null || (patch.Prefix == null && patch.Postfix == null && patch.Transpiler == null))
            {
                Console.WriteLine("A patch provided is likely invalid!");
            }

            return harmony.Patch(patch.Target,
                patch.Prefix != null ? new HarmonyMethod(patch.Prefix) : null,
                patch.Postfix != null ? new HarmonyMethod(patch.Postfix) : null,
                patch.Transpiler != null ? new HarmonyMethod(patch.Transpiler) : null,
                null);
        }
    }
}
