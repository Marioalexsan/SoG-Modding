using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.Modding
{
    public class GrindScript
    {
        private readonly Harmony harmony = new Harmony("GrindScriptPatcher");

        private readonly List<BaseScript> _loadedScripts = new List<BaseScript>();
        private Assembly _gameAssembly;
        private IEnumerable<TypeInfo> _gameTypes;
        internal ModLibrary Library { get; private set; }

        public static Game1 Game { get; private set; }
        public static GrindScript ModAPI { get; private set; }
        internal static ModLibrary ModLib { get => ModAPI.Library; }

        private GrindScript()
        {
            ModAPI = this;
            _gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");
            _gameTypes = _gameAssembly.DefinedTypes;
            ApplyPatches();
        }

        // Call this via reflection from launcher
        private static void Prepare()
        {
            if (ModAPI != null)
            {
                Console.WriteLine("GrindScript::Prepare() - A GrindScript instance already exists!");
                return;
            }
            Console.WriteLine("Preparing Grindscript...");

            new GrindScript();
        }

        // Call this via reflection from Game1::Initialize() patch
        private static void Initialize()
        {
            if (ModAPI == null)
            {
                Console.WriteLine("GrindScript::Initialize() - No GrindScript instance to init!");
                return;
            }
            Console.WriteLine("Initializing Grindscript...");
            Game = (Game1)ModAPI.GetGameType("SoG.Program").GetField("game", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            ModAPI.Library = new ModLibrary();
            ModAPI.LoadMods();
        }

        public TypeInfo GetGameType(string name)
        {
            return _gameTypes.First(t => t.FullName == name);
        }

        private bool LoadMod(string name)
        {
            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("ModContent");

            Console.WriteLine("Loading mod " + name);
            try
            {
                Assembly assembly = Assembly.LoadFile(name);
                Type type = assembly.GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript script = (BaseScript)type?.GetConstructor(new Type[] { })?.Invoke(new object[] { });

                _loadedScripts.Add(script);

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

        private bool LoadMods()
        {
            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            foreach (var file in Directory.GetFiles(dir))
            {
                LoadMod(file);
            }

            return true;
        }

        internal IEnumerable<BaseScript> GetLoadedMods()
        {
            return _loadedScripts;
        }

        private void ApplyPatches()
        {
            Console.WriteLine("Applying Patches...");
            PatchCodex.Patches[] toPatch = new PatchCodex.Patches[]
            {
                PatchCodex.Patches.Game1_Initialize,
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
                // TODO Codex patches
            };
            try
            {
                foreach (var id in toPatch)
                {
                    Console.WriteLine("Patch: " + id);
                    harmony.Patch(PatchCodex.GetPatch(id));
                }
                Console.WriteLine("Patches Applied!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to apply patches!\n" + e);
            }
            
            // Callbacks.InitializeUniquePatches();
        }
    }
}
