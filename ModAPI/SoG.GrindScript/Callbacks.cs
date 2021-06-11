using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyLib;
using Microsoft.Xna.Framework;

//https://stackoverflow.com/questions/7299097/dynamically-replace-the-contents-of-a-c-sharp-method
namespace SoG.GrindScript
{

    public class Callbacks
    {
        /*
        public static void InitializeUniquePatches()
        {
            try
            {
                // GetItemInstance prefix patch
                var prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetItemInstance_PrefixPatch");
                var original = Utils.GetGameType("SoG.ItemCodex").GetMethod("GetItemInstance");

                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            try
            {
                // GetEquipmentInfo prefix patches
                // (EquipmentCodex declares 4 functions, so I'm patching all 4 with the same function. Yeehaw.)
                // (it should work the same, since the functions effectively act as separate storage mediums)
                // (the patches just change the storage in question to a shared dictionary)
                var prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetEquipmentInfo_PrefixPatch");

                var original = typeof(EquipmentCodex).GetMethod("GetArmorInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = typeof(EquipmentCodex).GetMethod("GetAccessoryInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = typeof(EquipmentCodex).GetMethod("GetShieldInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                original = typeof(EquipmentCodex).GetMethod("GetShoesInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));


                // Facegear Codex's GetHatInfo patch
                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetFacegearInfo_PrefixPatch");

                original = typeof(FacegearCodex).GetMethod("GetHatInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Hat Codex's GetHatInfo patch
                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetHatInfo_PrefixPatch");

                original = typeof(HatCodex).GetMethod("GetHatInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                // Weapon Codex's GetWeaponInfo patch
                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("GetWeaponInfo_PrefixPatch");

                original = typeof(WeaponCodex).GetMethod("GetWeaponInfo");
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("LoadBatch_PrefixOverwrite");

                original = typeof(WeaponAssets.WeaponContentManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(item => item.Name == "LoadBatch").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(ItemHelper).GetTypeInfo().GetPrivateStaticMethod("_Animations_GetAnimationSet_PrefixOverwrite");

                original = typeof(Game1).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.Name == "_Animations_GetAnimationSet").ElementAt(1);
                harmony.Patch(original, new HarmonyMethod(prefix));
            
                prefix = typeof(EnemyHelper).GetTypeInfo().GetPrivateStaticMethod("GetEnemyInstance_PrefixPatch");

                original = typeof(EnemyCodex).GetMethod("GetEnemyInstance", BindingFlags.Public | BindingFlags.Static);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(EnemyHelper).GetTypeInfo().GetPrivateStaticMethod("_Enemy_AdjustForDifficulty_PrefixPatch");

                original = typeof(Game1).GetMethod("_Enemy_AdjustForDifficulty", BindingFlags.Public | BindingFlags.Instance);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(EnemyHelper).GetTypeInfo().GetPrivateStaticMethod("_Enemy_MakeElite_PrefixPatch");

                original = typeof(Game1).GetMethod("_Enemy_MakeElite", BindingFlags.Public | BindingFlags.Instance);
                harmony.Patch(original, new HarmonyMethod(prefix));

                prefix = typeof(DynEnvHelper).GetTypeInfo().GetPrivateStaticMethod("GetObjectInstance_PrefixPatch");

                original = typeof(DynamicEnvironmentCodex).GetMethod("GetObjectInstance", BindingFlags.Public | BindingFlags.Static);
                harmony.Patch(original, new HarmonyMethod(prefix));
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void InitializeLoadPatch()
        {
            try
            {
                // GetItemInstance prefix patch
                //var postfix = typeof(NativeInterface).GetTypeInfo().GetMethod("LoadGrindscript");
                var transpiler = typeof(Callbacks).GetTypeInfo().GetMethod("InitializeLoadPatch_Transpiler", BindingFlags.Public | BindingFlags.Static);
                var original = Utils.GetGameType("SoG.Game1").GetMethod("__StartupThreadExecute", BindingFlags.Instance | BindingFlags.Public);

                harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion*/
    }
}

