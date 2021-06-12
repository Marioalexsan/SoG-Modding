using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public static class PatchCodex
    {
        public enum Patches
        {
            // GrindScript
            Game1_Run,
            // BaseScript callbacks
            Game1_FinalDraw,
            Game1_Player_TakeDamage,
            Game1_Player_KillPlayer,
            Game1_Player_ApplyLvUpBonus,
            Game1_Enemy_TakeDamage,
            Game1_NPC_TakeDamage,
            Game1_NPC_Interact,
            Game1_LevelLoading_DoStuff_Arcadia,
            Game1_Chat_ParseCommand,
            Game1_Item_Use
            // API callbacks
        }

        public class PatchDescription
        {
            public MethodInfo Target;
            public MethodInfo Prefix;
            public MethodInfo Transpiler;
            public MethodInfo Postfix;
        }

        public static PatchDescription GetPatch(Patches which)
        {
            GrindScript API = GrindScript.ModAPI;
            TypeInfo Methods = typeof(PatchMethods).GetTypeInfo(); // Commonly used
            TypeInfo Game1 = API.GetGameType("SoG.Game1"); // Commonly used

            PatchDescription patch = new PatchDescription();
            switch (which)
            {
                case Patches.Game1_Run:
                    patch.Target = Game1.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnGame1Run");
                    break;
                case Patches.Game1_FinalDraw:
                    patch.Target = Game1.GetPublicInstanceMethod("FinalDraw");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnFinalDrawPrefix");
                    break;
                case Patches.Game1_Player_TakeDamage:
                    patch.Target = Game1.GetPublicInstanceMethod("_Player_TakeDamage");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnPlayerTakeDamagePrefix");
                    break;
                case Patches.Game1_Player_KillPlayer:
                    patch.Target = Game1.GetMethods(BindingFlags.Instance | BindingFlags.Public).First(m => m.Name == "_Player_KillPlayer" && m.GetParameters().Count() > 1);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnPlayerKilledPrefix");
                    break;
                case Patches.Game1_Player_ApplyLvUpBonus:
                    patch.Target = Game1.GetPublicInstanceMethod("_Player_ApplyLvUpBonus");
                    patch.Postfix = Methods.GetPrivateStaticMethod("PostPlayerLevelUp");
                    break;
                case Patches.Game1_Enemy_TakeDamage:
                    patch.Target = Game1.GetPublicInstanceMethod("_Enemy_TakeDamage");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnEnemyTakeDamagePrefix");
                    break;
                case Patches.Game1_NPC_TakeDamage:
                    patch.Target = Game1.GetPublicInstanceMethod("_NPC_TakeDamage");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnNPCTakeDamagePrefix");
                    break;
                case Patches.Game1_NPC_Interact:
                    patch.Target = Game1.GetPublicInstanceMethod("_NPC_Interact");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnNPCInteractionPrefix");
                    break;
                case Patches.Game1_LevelLoading_DoStuff_Arcadia:
                    patch.Target = Game1.GetPublicInstanceMethod("_LevelLoading_DoStuff_Arcadia");
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnArcadiaLoadPrefix");
                    break;
                case Patches.Game1_Chat_ParseCommand:
                    patch.Target = Game1.GetPublicInstanceMethod("_Chat_ParseCommand");
                    patch.Transpiler = Methods.GetPrivateStaticMethod("Chat_ParseCommandTranspiler");
                    break;
                case Patches.Game1_Item_Use:
                    patch.Target = Game1.GetDeclaredMethods("_Item_Use").ElementAt(1);
                    patch.Prefix = Methods.GetPrivateStaticMethod("OnItemUsePrefix");
                    break;
                default:
                    patch = null;
                    break;
            }

            return patch;
        }
    }
}
