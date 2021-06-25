using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CodeList = System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction>;

namespace SoG.Modding
{
    /// <summary>
    /// Contains transpiling methods used to modify vanilla code in more detail.
    /// Throughout the code, CodeList is a shortcut for IEnumerable of CodeInstructions.
    /// </summary>

    public static class Transpilers
    {
        /// <summary>
        /// Inserts <see cref="Callbacks.OnContentLoad"/> in <see cref="Game1.__StartupThreadExecute"/>.
        /// Insertion is done before <see cref="DialogueCharacterLoading.Init"/>.
        /// </summary>

        private static CodeList StartupTranspiler(CodeList code, ILGenerator gen)
        {
            MethodInfo target = typeof(DialogueCharacterLoading).GetMethod("Init");

            List<CodeInstruction> insert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, typeof(Callbacks).GetPrivateStaticMethod("OnContentLoad"))
            };

            return PatchUtils.InsertAfterMethod(code, target, insert);
        }

        /// <summary>
        /// Inserts <see cref="Callbacks.OnChatParseCommand"/> in <see cref="Game1._Chat_ParseCommand"/>.
        /// If a mod command runs, vanilla commands are skipped.
        /// </summary>

        private static CodeList CommandTranspiler(CodeList code, ILGenerator gen)
        {
            Label afterRet = gen.DefineLabel();

            MethodInfo target = typeof(string).GetMethod("ToLowerInvariant");

            List<CodeInstruction> insert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldarg_S, 1),
                new CodeInstruction(OpCodes.Ldarg_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(Callbacks).GetPrivateStaticMethod("OnChatParseCommand")),
                new CodeInstruction(OpCodes.Brfalse, afterRet),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Nop).WithLabels(afterRet)
            };

            return PatchUtils.InsertAfterMethod(code, target, insert);
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlayCue"/> and its variants so they can support modded sounds.
        /// </summary>

        private static CodeList PlayEffectTranspiler(CodeList code, ILGenerator gen)
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
                new CodeInstruction(OpCodes.Call, typeof(Utils).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex), 
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(ModContent).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            code = PatchUtils.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            return PatchUtils.InsertBeforeMethod(code, target, insertBefore); 
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlayCue"/> and its variants so they can support modded sounds.
        /// </summary>

        private static CodeList GetEffectTranspiler(CodeList code, ILGenerator gen)
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
                new CodeInstruction(OpCodes.Call, typeof(Utils).GetMethod("GetEffectSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(ModContent).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            code = PatchUtils.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            return PatchUtils.InsertBeforeMethod(code, target, insertBefore);
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.ReadySongInCue"/> so it can support modded music.
        /// </summary>

        private static CodeList GetMusicTranspiler(CodeList code, ILGenerator gen)
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
                new CodeInstruction(OpCodes.Call, typeof(Utils).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(ModContent).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            code = PatchUtils.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            return PatchUtils.InsertBeforeMethod(code, target, insertBefore);
        }

        /// <summary>
        /// Patches <see cref="SoundSystem.PlayMixCues"/> so it can support modded music.
        /// </summary>

        private static CodeList PlayMixTranspiler(CodeList code, ILGenerator gen)
        {
            Label skipVanillaBank = gen.DefineLabel();
            Label doVanillaBank = gen.DefineLabel();
            LocalBuilder modBank = gen.DeclareLocal(typeof(SoundBank));

            MethodInfo target = typeof(SoundBank).GetMethod("GetCue", new Type[] { typeof(string) });

            List<CodeInstruction> insertBefore = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(Utils).GetMethod("GetMusicSoundBank")),
                new CodeInstruction(OpCodes.Stloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse, doVanillaBank),
                new CodeInstruction(OpCodes.Ldloc_S, modBank.LocalIndex),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(ModContent).GetMethod("GetCueName")),
                new CodeInstruction(OpCodes.Call, target),
                new CodeInstruction(OpCodes.Br, skipVanillaBank),
                new CodeInstruction(OpCodes.Nop).WithLabels(doVanillaBank)
            };

            List<CodeInstruction> insertAfter = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Nop).WithLabels(skipVanillaBank)
            };

            // First method, index 0

            code = PatchUtils.InsertAfterMethod(code, target, insertAfter, missingPopIsOk: true);
            code = PatchUtils.InsertBeforeMethod(code, target, insertBefore);

            // Second method. Since we inserted the same method we search for, the actual index is 2, not 1

            code = PatchUtils.InsertAfterMethod(code, target, insertAfter, methodIndex: 2, missingPopIsOk: true);
            return PatchUtils.InsertBeforeMethod(code, target, insertBefore, methodIndex: 2);
        }
    }
}
