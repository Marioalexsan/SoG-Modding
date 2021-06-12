using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SoG.GrindScript
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

            CreateMissingDirectories();
            ApplyPatches();
        }

        public static void InitializeInLauncher()
        {
            if (ModAPI != null)
            {
                Console.WriteLine("Tried to InitializeInLauncher while a GrindScript instance exists!");
                return;
            }
            Console.WriteLine("Initializing Grindscript...");
            new GrindScript();
        }

        public static void InitializeInSoG()
        {
            if (ModAPI == null)
            {
                Console.WriteLine("Tried to InitializeInlauncher without a GrindScript instance!");
                return;
            }
            Game = (Game1)ModAPI.GetGameType("SoG.Program").GetField("game", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            ModAPI.Library = new ModLibrary();
            ModAPI.LoadMods();
        }

        private static void CreateMissingDirectories()
        {
            try
            {
                Directory.CreateDirectory("Mods");
            }
            catch (Exception) { }
            try
            {
                Directory.CreateDirectory("ModContent");
            }
            catch (Exception) { }
        }

        public TypeInfo GetGameType(string name)
        {
            return _gameTypes.First(t => t.FullName == name);
        }

        private bool LoadMod(string name)
        {
            CreateMissingDirectories();

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
                PatchCodex.Patches.Game1_Run,
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
                    Console.WriteLine("Patch" + id);
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
