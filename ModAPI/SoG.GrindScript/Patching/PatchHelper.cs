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

        /// <summary>
        /// <para> Transpiles the given instruction set by inserting code instructions after the target method. </para>
        /// <para> If there are multiple calls of the target method, whichMethod will specify the one to pick. whichMethod = 1 will insert after the first occurence, for example. </para>
        /// <para> 
        /// If the target method has a return value that is being used by subsquent code, you can specify ignoreLackOfPop = true to force the insertion.
        /// This is useful if you also use other insertions to create a ternary operator.
        /// </para>
        /// </summary>
        /// <returns> The modified code, with new instructions inserted as described. </returns>
        /// <exception cref="Exception"> Thrown if the transpile fails due to the described incompatibilities. </exception>
        /// 

        public static IEnumerable<CodeInstruction> InsertAfterMethod(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodInfo target, IEnumerable<CodeInstruction> codeToInsert, int whichMethod, bool missingPopIsOk = false)
        {
            int counter = whichMethod;
            var noReturnValue = target.ReturnType == typeof(void);
            int stage = 0;
            foreach (CodeInstruction ins in instructions)
            {
                if (stage == 0)
                {
                    if (ins.Calls(target) && --counter == 0) 
                        stage = 1;
                }
                else if (stage == 1)
                {
                    if (!(ins.opcode == OpCodes.Pop || noReturnValue || missingPopIsOk)) 
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

        /// <summary> 
        /// <para> Transpiles the given instruction set by inserting code instructions before the target method. </para>
        /// <para> If there are multiple calls of the target method, whichMethod will specify the one to pick. whichMethod = 1 will insert before the first occurence, for example. </para>
        /// </summary>
        /// <returns> The modified code, with new instructions inserted as described. </returns>
        /// <exception cref="Exception"> Thrown if the transpile fails due to failing to find the target method, or if a suitable insertion point wasn't spotted. </exception>

        public static IEnumerable<CodeInstruction> InsertBeforeMethod(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodInfo target, IEnumerable<CodeInstruction> codeToInsert, int whichMethod)
        {
            List<CodeInstruction> codeStore = new List<CodeInstruction>();
            List<CodeInstruction> leftoverCode = new List<CodeInstruction>();
            int counter = whichMethod;
            int stage = 0;

            foreach (CodeInstruction ins in instructions)
            {
                if (stage == 0 && ins.Calls(target) && --counter == 0) stage = 1;
                if (stage == 0) codeStore.Add(ins);
                else leftoverCode.Add(ins);
            }
            if (stage != 1) throw new Exception("Transpile failed: couldn't find target!");

            
            int insertIndex = codeStore.Count;
            int stackDelta = -1 * target.GetParameters().Length;
            if ((target.CallingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
                stackDelta -= 1; // Account for "this"

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
