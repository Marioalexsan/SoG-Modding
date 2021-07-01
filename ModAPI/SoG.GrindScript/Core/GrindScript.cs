using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.Modding
{
    /// <summary>
    /// Core of the API
    /// </summary>

    public static class GrindScript
    {
        internal static Texture2D MissingTex { get; private set; }

        private static readonly Harmony _harmony = new Harmony("GrindScriptPatcher");

        internal static readonly List<BaseScript> _loadedScripts = new List<BaseScript>();

        private static Assembly _gameAssembly;

        private static IEnumerable<TypeInfo> _gameTypes;

        public static Game1 Game { get; private set; }

        internal static string GSCommand = "GrindScript";

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

            // Set up a cooler null tex for missing assets
            MissingTex = Utils.TryLoadTex("ModContent/GrindScript/NullTexGS", GrindScript.Game.Content);
            
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

            List<PatchCodex.PatchID> nullPatches = new List<PatchCodex.PatchID>();
            int successCount = 0;
            int totalCount = 0;
            int nextProgressUpdate = 20;

            var allPatches = Enum.GetValues(typeof(PatchCodex.PatchID));
            foreach (PatchCodex.PatchID id in allPatches)
            {
                PatchCodex.PatchInfo patch = PatchCodex.GetPatch(id);
                totalCount++;

                if (patch != null)
                {
                    try
                    {
                        if (patch.Target == null || (patch.Prefix == null && patch.Postfix == null && patch.Transpiler == null))
                            GrindScript.Logger.Warn($"Patch {id} may be invalid!");

                        _harmony.Patch(patch);
                        successCount++;

                        if (totalCount * 100 / allPatches.Length >= nextProgressUpdate)
                        {
                            Logger.Info($"{nextProgressUpdate}%...");
                            nextProgressUpdate += 20;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Patch {id} threw an exception! Message: {e.Message}");
                    }
                }
                else nullPatches.Add(id);
            }
            
            int nullCount = nullPatches.Count;
            if (nullCount > 0)
            {
                Logger.Info($"{nullCount} null patches were encountered:");

                int index = -1;
                const int toDisplay = 3;

                while (++index < toDisplay)
                    Logger.Info("\t" + nullPatches[index]);

                if (nullCount > toDisplay)
                    Logger.Info($"\tand {nullCount - toDisplay} more...");
            }

            Logger.Info($"Applied {successCount} patches successfully!");
        }

        /// <summary>
        /// Loads a mod and instantiates its BaseScript derived class (if any).
        /// </summary>

        private static void LoadMod(string name)
        {
            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("Content/ModContent");

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
        /// Prepares commands that come bundled with GrindScript. These can be accessed using "/GrindScript:{command}"
        /// </summary>

        private static void SetupCommands()
        {
            var parsers = ModLibrary.Commands[GSCommand] = new Dictionary<string, CommandParser>();

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
                    commandList = ModLibrary.Commands[GSCommand];
                }
                else if(!ModLibrary.Commands.TryGetValue(args[0], out commandList))
                {
                    CAS.AddChatMessage($"[{GSCommand}] Unknown mod!");
                    return;
                }
                CAS.AddChatMessage($"[{GSCommand}] Command list{(args.Length == 0 ? "" : $" for {args[0]}" )}:");

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

            parsers["PlayerPos"] = (_1, _2) =>
            {
                var local = Game.xLocalPlayer.xEntity.xTransform.v2Pos;

                CAS.AddChatMessage($"[{GSCommand}] Player position: {(int)local.X}, {(int)local.Y}");
            };

            parsers["ModTotals"] = (message, _2) =>
            {
                var args = Utils.GetArgs(message);
                if (args.Length != 1)
                {
                    CAS.AddChatMessage($"[{GSCommand}] Usage: /GrindScript:ModTotals <unique type>");
                }

                switch (args[0])
                {
                    case "Items":
                        CAS.AddChatMessage($"[{GSCommand}] Items defined: " + ModLibrary.GlobalLib.Items.Count);
                        break;
                    case "Perks":
                        CAS.AddChatMessage($"[{GSCommand}] Perks defined: " + ModLibrary.GlobalLib.Perks.Count);
                        break;
                    case "Treats":
                        CAS.AddChatMessage($"[{GSCommand}] TreatsCurses defined: " + ModLibrary.GlobalLib.TreatsCurses.Count);
                        break;
                    default:
                        CAS.AddChatMessage($"[{GSCommand}] Usage: /GrindScript:ModTotals <unique type>");
                        break;
                }
            };
        }
    }
}
