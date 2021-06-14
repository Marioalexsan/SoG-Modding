using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.Modding
{
    /// <summary>
    /// Core of the API
    /// </summary>
    public static class GrindScript
    {
        private static int _launchState = 0;

        private static readonly Harmony _harmony = new Harmony("GrindScriptPatcher");

        internal static readonly List<BaseScript> LoadedScripts = new List<BaseScript>();
        private static Assembly _gameAssembly;
        private static IEnumerable<TypeInfo> _gameTypes;

        public static Game1 Game { get; private set; }

        // Call this via reflection from launcher
        private static void Prepare()
        {
            Console.WriteLine("Preparing Grindscript...");
            if (_launchState != 0)
            {
                Console.WriteLine("Can't proceed because launch state is " + _launchState + "!");
                return;
            }

            _launchState = 1;
            _gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");
            _gameTypes = _gameAssembly.DefinedTypes;

            ApplyPatches();
        }

        // Call this via reflection from Game1::Initialize() patch
        private static void Initialize()
        {
            Console.WriteLine("Initializing Grindscript...");
            if (_launchState != 1)
            {
                Console.WriteLine("Can't proceed because launch state is " + _launchState + "!");
                return;
            }

            Game = (Game1)GetGameType("SoG.Program").GetField("game", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            LoadMods();
        }

        public static TypeInfo GetGameType(string name)
        {
            return _gameTypes.First(t => t.FullName == name);
        }

        private static void ApplyPatches()
        {
            Console.WriteLine("Applying Patches...");
            PatchCodex.Patches[] toPatch = new PatchCodex.Patches[]
            {
                PatchCodex.Patches.Game1_Initialize,
                PatchCodex.Patches.Game1_StartupThreadExecute,
                PatchCodex.Patches.Game1_FinalDraw,
                PatchCodex.Patches.Game1_Player_TakeDamage,
                PatchCodex.Patches.Game1_Player_KillPlayer,
                PatchCodex.Patches.Game1_Player_ApplyLvUpBonus,
                PatchCodex.Patches.Game1_Enemy_TakeDamage,
                PatchCodex.Patches.Game1_NPC_TakeDamage,
                PatchCodex.Patches.Game1_NPC_Interact,
                PatchCodex.Patches.Game1_LevelLoading_DoStuff_Arcadia,
                PatchCodex.Patches.Game1_Chat_ParseCommand,
                PatchCodex.Patches.Game1_Item_Use,
                // Item API Patches
                PatchCodex.Patches.ItemCodex_GetItemDescription,
                PatchCodex.Patches.ItemCodex_GetItemInstance,
                PatchCodex.Patches.EquipmentCodex_GetArmorInfo,
                PatchCodex.Patches.EquipmentCodex_GetAccessoryInfo,
                PatchCodex.Patches.EquipmentCodex_GetShieldInfo,
                PatchCodex.Patches.EquipmentCodex_GetShoesInfo,
                PatchCodex.Patches.HatCodex_GetHatInfo,
                PatchCodex.Patches.FacegearCodex_GetHatInfo,
                PatchCodex.Patches.WeaponCodex_GetWeaponInfo,
                PatchCodex.Patches.WeaponContentManager_LoadBatch,
                PatchCodex.Patches.Game1_Animations_GetAnimationSet
            };
            try
            {
                foreach (var id in toPatch)
                {
                    Console.WriteLine("Patch: " + id);
                    _harmony.Patch(PatchCodex.GetPatch(id));
                }
                Console.WriteLine("Patches Applied!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to apply patches!\n" + e);
            }
        }

        public static bool LoadMod(string name)
        {
            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("ModContent");

            Console.WriteLine("Loading mod " + name);
            try
            {
                Assembly assembly = Assembly.LoadFile(name);
                Type type = assembly.GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript script = (BaseScript)type?.GetConstructor(new Type[] { })?.Invoke(new object[] { });

                LoadedScripts.Add(script);

                Console.WriteLine("Loaded mod " + name);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load mod" + name);
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool LoadMods()
        {
            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            foreach (var file in Directory.GetFiles(dir))
            {
                LoadMod(file);
            }

            return true;
        }
    }
}
