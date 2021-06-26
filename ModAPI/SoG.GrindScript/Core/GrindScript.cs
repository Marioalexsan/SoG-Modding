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
        private static readonly Harmony _harmony = new Harmony("GrindScriptPatcher");

        internal static readonly List<BaseScript> _loadedScripts = new List<BaseScript>();

        private static Assembly _gameAssembly;

        private static IEnumerable<TypeInfo> _gameTypes;

        public static Game1 Game { get; private set; }

        internal static ConsoleLogger Logger { get; private set; } = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, "GrindScript");

        /// <summary>
        /// Prepares GrindScript by doing some processing before SoG's Main method runs.
        /// </summary>

        private static void Prepare()
        {
            Logger.Info("Preparing Grindscript...");

            _gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");
            _gameTypes = _gameAssembly.DefinedTypes;

            ApplyPatches();
            SetupCommands();
        }

        /// <summary>
        /// Initializes GrindScript during SoG's startup thread
        /// </summary>

        private static void Initialize()
        {
            if (_gameTypes == null)
            {
                Logger.Error("Can not start GrindScript because it hasn't been prepared!");
                return;
            }

            Logger.Info("Initializing Grindscript...");

            Game = (Game1)GetGameType("SoG.Program").GetField("game", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            
            // Modded content resides in a separate folder to avoid breaking vanilla stuff accidentally.
            Game.sAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GrindScript/";

            // It is not a good idea for modded runs to submit scores.
            Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;

            LoadMods();
        }

        /// <summary>
        /// Gets a game type via reflection. Not needed if you have a reference to Secrets of Grindea.exe
        /// </summary>

        public static TypeInfo GetGameType(string name)
        {
            return _gameTypes.First(t => t.FullName == name);
        }

        /// <summary>
        /// Applies all patches found in PatchCodex.
        /// </summary>

        private static void ApplyPatches()
        {
            Logger.Info("Applying Patches...");
            try
            {
                foreach (PatchCodex.PatchID id in Enum.GetValues(typeof(PatchCodex.PatchID)))
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

        /// <summary>
        /// Loads a mod and instantiates its BaseScript derived class (if any).
        /// </summary>

        private static void LoadMod(string name)
        {
            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("ModContent");

            Logger.Info("Loading mod " + name);
            try
            {
                Assembly assembly = Assembly.LoadFile(name);
                Type type = assembly.GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript script = (BaseScript)type?.GetConstructor(new Type[] { })?.Invoke(new object[] { });

                _loadedScripts.Add(script);

                Logger.Info("Loaded mod " + name);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load mod {name}. Exception message: {e.Message}");
            }
        }

        /// <summary>
        /// Loads all mods found in the "/Mods" directory
        /// </summary>

        private static void LoadMods()
        {
            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            foreach (var file in Directory.GetFiles(dir))
            {
                LoadMod(file);
            }
        }

        /// <summary>
        /// Prepares commands that come bundled with GrindScript. These can be accessed using "/GScript:{command}"
        /// </summary>

        private static void SetupCommands()
        {
            var parsers = ModLibrary.Global.ModCommands["GScript"] = new Dictionary<string, CommandParser>();

            parsers["ModList"] = (_1, _2) =>
            {
                CAS.AddChatMessage($"[GrindScript] Mod Count: {_loadedScripts.Count}");

                var messages = new List<string>();
                var concated = "";
                foreach (var mod in _loadedScripts)
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

            parsers["Help"] = (message, _2) =>
            {
                Dictionary<string, CommandParser> commandList = null;
                var args = Utils.GetArgs(message);
                if (args.Length == 0)
                {
                    commandList = ModLibrary.Global.ModCommands["GScript"];
                }
                else if(!ModLibrary.Global.ModCommands.TryGetValue(args[0], out commandList))
                {
                    CAS.AddChatMessage("[GrindScript] Unknown mod!");
                    return;
                }
                CAS.AddChatMessage($"[GrindScript] Command list{(args.Length == 0 ? "" : $" for {args[0]}" )}:");

                var messages = new List<string>();
                var concated = "";
                foreach (var cmd in commandList.Keys)
                {
                    if (concated.Length + cmd.Length > 40)
                    {
                        messages.Add(concated);
                        concated = "";
                    }
                    concated += cmd + " ";
                }
                if (concated != "")
                    messages.Add(concated);

                foreach (var line in messages)
                    CAS.AddChatMessage(line);
            };
        }
    }
}
