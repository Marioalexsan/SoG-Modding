using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SoG.Modding
{
    public static class PatchHelper
    {
        private static readonly Dictionary<StackBehaviour, int> __stackDeltas = new Dictionary<StackBehaviour, int>
        {
            { StackBehaviour.Pop0, 0 },
            { StackBehaviour.Pop1, -1 },
            { StackBehaviour.Pop1_pop1, -2 },
            { StackBehaviour.Popi, -1 },
            { StackBehaviour.Popi_pop1, -2 },
            { StackBehaviour.Popi_popi, -2 },
            { StackBehaviour.Popi_popi_popi, -3 },
            { StackBehaviour.Popi_popi8, -2 },
            { StackBehaviour.Popi_popr4, -2 },
            { StackBehaviour.Popi_popr8, -2 },
            { StackBehaviour.Popref, -1 },
            { StackBehaviour.Popref_pop1, -2 },
            { StackBehaviour.Popref_popi, -2 },
            { StackBehaviour.Popref_popi_pop1, -3 },
            { StackBehaviour.Popref_popi_popi, -3 },
            { StackBehaviour.Popref_popi_popi8, -3 },
            { StackBehaviour.Popref_popi_popr4, -3 },
            { StackBehaviour.Popref_popi_popr8, -3 },
            { StackBehaviour.Popref_popi_popref, -3 },
            { StackBehaviour.Push0, 0 },
            { StackBehaviour.Push1, 1 },
            { StackBehaviour.Push1_push1, 2 },
            { StackBehaviour.Pushi, 1 },
            { StackBehaviour.Pushi8, 1 },
            { StackBehaviour.Pushr4, 1 },
            { StackBehaviour.Pushr8, 1 },
            { StackBehaviour.Pushref, 1 },
            { StackBehaviour.Varpop, -1 },
            { StackBehaviour.Varpush, 1 }
        };

        public static void LogCodeAroundTarget(IEnumerable<CodeInstruction> instructions, MethodInfo target, int previous = 3, int following = 3)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            bool found = false;
            for (int index = 0; index < list.Count; index++)
            {
                if (list[index].Calls(target))
                {
                    found = true;
                    Console.WriteLine("\n===== Code around method call " + target.DeclaringType.Name + "::" + target.Name + " =====");
                    for (int scanIndex = Math.Max(0, index - previous); scanIndex <= Math.Min(list.Count - 1, index + following); scanIndex++)
                    {
                        Console.WriteLine(list[scanIndex].ToString());
                    }
                    Console.WriteLine("===== Code end =====\n");
                    break;
                }
            }
            if (!found) Console.WriteLine("Didn't find class " + target.DeclaringType.Name + "::" + target.Name + " to log code around of.");
        }

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
                if (stage == 0)
                {
                    if (ins.Calls(target)) 
                        stage = 1;
                }
                else if (stage == 1)
                {
                    if (!(ins.opcode == OpCodes.Pop || noReturnValue)) 
                        throw new Exception("Transpile failed: insert target has return value that is being used!");

                    stage = 2;

                    if (ins.opcode == OpCodes.Pop) 
                        yield return ins;
                    foreach (CodeInstruction newIns in codeToInsert) 
                        yield return newIns;
                    if (ins.opcode == OpCodes.Pop)
                        continue;
                }
                yield return ins;
            }
            if (stage != 2) throw new Exception("Transpile failed: couldn't find target!");
        }

        public static IEnumerable<CodeInstruction> InsertBeforeFirstMethod(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodInfo target, IEnumerable<CodeInstruction> codeToInsert)
        {
            List<CodeInstruction> codeStore = new List<CodeInstruction>();
            List<CodeInstruction> leftoverCode = new List<CodeInstruction>();
            int stage = 0;

            // Step 1: find target
            foreach (CodeInstruction ins in instructions)
            {
                if (stage == 0 && ins.Calls(target)) stage = 1;
                if (stage == 0) codeStore.Add(ins);
                else leftoverCode.Add(ins);
            }
            if (stage != 1) throw new Exception("Transpile failed: couldn't find target!");

            // Step 2: compute starting pushes
            int stackDelta = -1 * target.GetParameters().Length;
            int insertIndex = codeStore.Count;

            // Step 3: go through previous instructions, adding stack deltas until we hit 0
            if (stackDelta != 0) 
            {
                for (insertIndex = codeStore.Count - 1; insertIndex >= 0; insertIndex--)
                {
                    var ins = codeStore[insertIndex];
                    stackDelta += __stackDeltas[ins.opcode.StackBehaviourPush] + __stackDeltas[ins.opcode.StackBehaviourPop];

                    if (stackDelta == 0)
                    {
                        stage = 2;
                        break;
                    }
                    if (stackDelta > 0)
                        throw new Exception("Transpile failed: stackDelta has positive value!");
                }
            }
            else stage = 2;

            if (stage != 2) throw new Exception("Transpile failed: couldn't calculate position before method!");

            // Step 4: insert the new IL, and return all of the goodies
            for (int index = 0; index < codeStore.Count; index++)
            {
                if (index == insertIndex)
                {
                    foreach (CodeInstruction ins in codeToInsert)
                        yield return ins;
                }
                yield return codeStore[index];
            }

            if (insertIndex == codeStore.Count)
            {
                foreach (CodeInstruction ins in codeToInsert)
                    yield return ins;
            }

            foreach (CodeInstruction ins in leftoverCode)
                yield return ins;
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
