using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CodeEnumerable = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;
using SoG.Modding.Utils;
using SoG.Modding.Extensions;

namespace SoG.Modding.Patches
{
    internal static partial class PatchCollection
    {
        /// <summary>
        /// Inserts <see cref="OnContentLoad"/> in <see cref="Game1.__StartupThreadExecute"/>.
        /// Insertion is done before <see cref="DialogueCharacterLoading.Init"/>.
        /// </summary>
        private static CodeEnumerable StartupTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            MethodInfo target = typeof(DialogueCharacterLoading).GetMethod("Init");

            List<CodeInstruction> insert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, typeof(PatchCollection).GetPrivateStaticMethod("OnContentLoad"))
            };

            return PatchTools.InsertAfterMethod(code, target, insert);
        }

        /// <summary>
        /// Inserts <see cref="OnChatParseCommand"/> in <see cref="Game1._Chat_ParseCommand"/>.
        /// If a mod command runs, vanilla commands are skipped.
        /// </summary>
        private static CodeEnumerable CommandTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            Label afterRet = gen.DefineLabel();

            MethodInfo target = typeof(string).GetMethod("ToLowerInvariant");

            List<CodeInstruction> insert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldarg_S, 1),
                new CodeInstruction(OpCodes.Ldarg_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(PatchCollection).GetPrivateStaticMethod("OnChatParseCommand")),
                new CodeInstruction(OpCodes.Brfalse, afterRet),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Nop).WithLabels(afterRet)
            };

            return PatchTools.InsertAfterMethod(code, target, insert);
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlayCue"/> and its variants
        /// so they can support modded sounds.
        /// </summary>
        private static CodeEnumerable PlayEffectTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            // Original: soundBank.PlayCue(sCueName)
            // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.PlayCue(sCueName) : soundBank.PlayCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("PlayCue", new Type[] { typeof(string) });

            List<CodeInstruction> insertBefore = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex), 
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            code = PatchTools.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            return PatchTools.InsertBeforeMethod(code, target, insertBefore); 
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlayCue"/> and its variants
        /// so they can support modded sounds.
        /// </summary>
        private static CodeEnumerable GetEffectTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            // Original: soundBank.GetCue(sCueName)
            // Modified: (local1 = GetEffectSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            List<CodeInstruction> insertBefore = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            code = PatchTools.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            return PatchTools.InsertBeforeMethod(code, target, insertBefore);
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.ReadySongInCue"/>
        /// so it can support modded music.
        /// </summary>
        private static CodeEnumerable GetMusicTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            // Original: musicBank.GetCue(sCueName)
            // Modified: (local1 = GetMusicSoundBank(sCueName)) != null ? local1.GetCue(sCueName) : soundBank.GetCue(sCueName)

            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            List<CodeInstruction> insertBefore = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            code = PatchTools.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            return PatchTools.InsertBeforeMethod(code, target, insertBefore);
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlayMixCues"/>
        /// so that it can support modded music.
        /// </summary>
        private static CodeEnumerable PlayMixTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            List<CodeInstruction> insertBefore = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(TranspilerAP).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            // First method, index 0

            code = PatchTools.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            code = PatchTools.InsertBeforeMethod(code, target, insertBefore);

            // Second method. Since we inserted the same method we search for, the actual index is 2, not 1

            code = PatchTools.InsertAfterMethod(code, target, insertAfter, methodIndex: 2, missingPopIsOk: true);
            return PatchTools.InsertBeforeMethod(code, target, insertBefore, methodIndex: 2);
        }

        /// <summary>
        /// Patches <see cref="Game1._LevelLoading_DoStuff(Level.ZoneEnum, bool)"/>
        /// so that it can support modded level setup.
        /// </summary>
        private static CodeEnumerable LevelDoStuffTranspiler(CodeEnumerable code, ILGenerator gen)
        {
            MethodInfo target = typeof(Quests.QuestLog).GetMethod("UpdateCheck_PlaceVisited");

            List<CodeInstruction> insert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_S, 1),
                new CodeInstruction(OpCodes.Ldarg_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(PatchCollection).GetPrivateStaticMethod("OnLevelLoadDoStuff"))
            };

            return PatchTools.InsertBeforeMethod(code, target, insert);
        }

    }
}
