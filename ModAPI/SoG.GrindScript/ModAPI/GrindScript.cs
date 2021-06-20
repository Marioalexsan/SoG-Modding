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
        internal static ConsoleLogger Logger { get; private set; } = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, "GrindScript");

        private static void Prepare()
        {
            Logger.Info("Preparing Grindscript...");

            if (_launchState != 0)
            {
                Logger.Fatal($"Can't proceed because launch state is {_launchState}!");
                return;
            }

            _launchState = 1;
            _gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");
            _gameTypes = _gameAssembly.DefinedTypes;

            ApplyPatches();
            InitializeCommands();
        }

        private static void Initialize()
        {
            Logger.Info("Initializing Grindscript...");
            if (_launchState != 1)
            {
                Logger.Fatal("Can't proceed because launch state is " + _launchState + "!");
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
            Logger.Info("Applying Patches...");
            try
            {
                foreach (PatchCodex.Patches id in Enum.GetValues(typeof(PatchCodex.Patches)))
                {
                    Logger.Info("Patch: " + id);
                    _harmony.Patch(PatchCodex.GetPatch(id));
                }
                Logger.Info("Patches Applied!");
            }
            catch (Exception e)
            {
                Logger.Fatal("Failed to apply patches!\n" + e);
            }
        }

        private static bool LoadMod(string name)
        {
            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("ModContent");

            Logger.Info("Loading mod " + name);
            try
            {
                Assembly assembly = Assembly.LoadFile(name);
                Type type = assembly.GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript script = (BaseScript)type?.GetConstructor(new Type[] { })?.Invoke(new object[] { });

                LoadedScripts.Add(script);

                Logger.Info("Loaded mod " + name);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load mod {name}. Exception message: {e.Message}");
                return false;
            }
        }

        private static bool LoadMods()
        {
            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            foreach (var file in Directory.GetFiles(dir))
            {
                LoadMod(file);
            }

            return true;
        }

        private static void InitializeCommands()
        {
            var parsers = ModLibrary.Global.ModCommands["Modding"] = new Dictionary<string, CommandParser>();

            parsers["ModList"] = (_1, _2) =>
            {
                CAS.AddChatMessage($"Mod Count: {LoadedScripts.Count}");

                var messages = new List<string>();
                var concated = "";
                foreach (var mod in LoadedScripts)
                {
                    string name = mod.GetType().Name;
                    if (concated.Length + name.Length > 40)
                    {
                        messages.Add(concated);
                        concated = "";
                    }
                    concated += mod.GetType().Name + " ";
                }
                if (concated != "")
                    messages.Add(concated);

                foreach (var line in messages)
                    CAS.AddChatMessage(line);
            };

            parsers["ModList"] = (_1, _2) =>
            {
                CAS.AddChatMessage($"Mod Count: {LoadedScripts.Count}");

                var messages = new List<string>();
                var concated = "";
                foreach (var mod in LoadedScripts)
                {
                    string name = mod.GetType().Name;
                    if (concated.Length + name.Length > 40)
                    {
                        messages.Add(concated);
                        concated = "";
                    }
                    concated += mod.GetType().Name + " ";
                }
                if (concated != "")
                    messages.Add(concated);

                foreach (var line in messages)
                    CAS.AddChatMessage(line);
            };
        }
    }
}
