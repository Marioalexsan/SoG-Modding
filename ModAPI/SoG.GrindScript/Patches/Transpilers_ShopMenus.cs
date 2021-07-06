using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using CodeEnumerable = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;
using System.Reflection.Emit;
using HarmonyLib;
using SoG.Modding.Utils;
using System.Diagnostics;

namespace SoG.Modding.Patches
{
    // Methods here are used as C# code injected via transpilers.
    internal static partial class PatchCollection
    {
		private static CodeEnumerable RenderTreatCurseAssignTranspiler(CodeEnumerable code, ILGenerator gen)
		{
			List<CodeInstruction> codeList = new List<CodeInstruction>(code);

			LocalBuilder start = gen.DeclareLocal(typeof(int));
			LocalBuilder end = gen.DeclareLocal(typeof(int));
			LocalBuilder worker = gen.DeclareLocal(typeof(TCMenuWorker));

			// Uncomment if you need the indexed IL
			// PatchTools.WriteILToDisk(code);

			var firstInsert = new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetProperty(nameof(TranspilerAP.TCMenuWorker)).GetGetMethod()),
				new CodeInstruction(OpCodes.Stloc_S, worker.LocalIndex),
				new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
				new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetMethod(nameof(TCMenuWorker.Update))),
				new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
				new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetProperty(nameof(TCMenuWorker.TCListStart)).GetGetMethod()),
				new CodeInstruction(OpCodes.Stloc_S, start.LocalIndex),
				new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
				new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetProperty(nameof(TCMenuWorker.TCListEnd)).GetGetMethod()),
				new CodeInstruction(OpCodes.Stloc_S, end.LocalIndex),
				new CodeInstruction(OpCodes.Ldloc_S, start.LocalIndex),
			};

			var secondInsert = new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Ldloc_S, end.LocalIndex)
			};

			var thirdInsert = new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Ldloc_S, worker.LocalIndex),
				new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetProperty(nameof(TranspilerAP.SpriteBatch)).GetGetMethod()),
				new CodeInstruction(OpCodes.Ldarg_1),
				new CodeInstruction(OpCodes.Ldarg_2),
				new CodeInstruction(OpCodes.Call, typeof(TCMenuWorker).GetMethod(nameof(TCMenuWorker.DrawScroller))),
			};

			var offsetInsert = new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Ldloc_S, start.LocalIndex),
				new CodeInstruction(OpCodes.Sub)
			};

			// Assert to check if underlying method hasn't shifted heavily
			OpCode op = OpCodes.Nop;
			Debug.Assert(PatchTools.TryILAt(code, 457, out op) && op == OpCodes.Ldarg_0, "Possible shift at 457");
			Debug.Assert(PatchTools.TryILAt(code, 451, out op) && op == OpCodes.Ldarg_0, "Possible shift at 451");
			Debug.Assert(PatchTools.TryILAt(code, 105, out op) && op == OpCodes.Ldc_I4_5, "Possible shift at 105");
			Debug.Assert(PatchTools.TryILAt(code, 94, out op) && op == OpCodes.Ldc_I4_5, "Possible shift at 94");
			Debug.Assert(PatchTools.TryILAt(code, 70, out op) && op == OpCodes.Ldc_I4_0, "Possible shift at 70");

			// These should be done from highest offset to lowest
			// to avoid accounting for previous inserts / removes

			// Inserts the DrawScroller method after the for
			code = PatchTools.InsertAt(code, thirdInsert, 457);

			// Replaces the for's condition with i < end
			code = PatchTools.ReplaceAt(code, 5, secondInsert, 451);

			// Inserts an offset for vector.X, vector.Y expressions
			code = PatchTools.InsertAt(code, offsetInsert, 105);
			code = PatchTools.InsertAt(code, offsetInsert, 94);

			// Initializes local fields, calls some TCMenuWorker methods, replaces the for's init with i = start
			code = PatchTools.ReplaceAt(code, 1, firstInsert, 70);

			return code;
		}
    }
}