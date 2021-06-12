using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SoG.GrindScript
{
    public static class PatchMethods
    {
        private static IEnumerable<CodeInstruction> InitializeLoadPatch_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo target = GrindScript.ModAPI.GetGameType("SoG.Game1").GetMethod("_MainMenu_PopulateCharacterSelect", BindingFlags.Public | BindingFlags.Instance);
            List<CodeInstruction> insertedCode = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, typeof(PatchMethods).GetTypeInfo().GetMethod("OnContentLoad", BindingFlags.NonPublic | BindingFlags.Static))
            };

            return PatchHelper.InsertAfterFirstMethod(instructions, generator, target, insertedCode);
        }

        private static void OnGame1Run()
        {
            GrindScript.InitializeInSoG();
        }

        private static void OnContentLoad()
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnCustomContentLoad();
        }

        private static void OnFinalDrawPrefix()
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnDraw();
        }

        private static void OnPlayerTakeDamagePrefix(ref int iInDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnPlayerDamaged(ref iInDamage, ref byType);
        }

        private static void OnPlayerKilledPrefix()
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnPlayerKilled();
        }

        private static void PostPlayerLevelUp(PlayerView xView)
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.PostPlayerLevelUp(xView);
        }

        private static void OnEnemyTakeDamagePrefix(Enemy xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnEnemyDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCTakeDamagePrefix(NPC xEnemy, ref int iDamage, ref byte byType)
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnNPCDamaged(xEnemy, ref iDamage, ref byType);
        }

        private static void OnNPCInteractionPrefix(PlayerView xView, NPC xNPC)
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnNPCInteraction(xNPC);
        }

        private static void OnArcadiaLoadPrefix()
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnArcadiaLoad();
        }

        private static IEnumerable<CodeInstruction> Chat_ParseCommandTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo target = typeof(string).GetMethod("ToLowerInvariant", BindingFlags.Public | BindingFlags.Instance);
            Label afterRet = generator.DefineLabel();
            List<CodeInstruction> insertedCode = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldarg_S, 1),
                new CodeInstruction(OpCodes.Ldarg_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(PatchMethods).GetTypeInfo().GetMethod("OnChatParseCommand", BindingFlags.NonPublic | BindingFlags.Static)),
                new CodeInstruction(OpCodes.Brfalse, afterRet),
                new CodeInstruction(OpCodes.Ret),
                new CodeInstruction(OpCodes.Nop).WithLabels(afterRet)
            };

            return PatchHelper.InsertAfterFirstMethod(instructions, generator, target, insertedCode);
        }

        private static bool OnChatParseCommand(string command, string message, int connection)
        {
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
            {
                if (mod.OnChatParseCommand(command, message, connection)) 
                    return true;
            }
            return false;
        }

        private static void OnItemUsePrefix(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            if (xView.xViewStats.bIsDead) return;
            foreach (BaseScript mod in GrindScript.ModAPI.GetLoadedMods())
                mod.OnItemUse(enItem, xView, ref bSend);
        }
    }
}
