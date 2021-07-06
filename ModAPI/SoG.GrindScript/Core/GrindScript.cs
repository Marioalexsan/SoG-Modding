using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using SoG.Modding.Utils;
using Microsoft.Xna.Framework.Content;
using SoG.Modding.Patches;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Core of the modding API.
    /// </summary>
    public class GrindScript
    {
        private readonly Harmony _harmony = new Harmony("GrindScriptPatcher");

        private readonly Stack<BaseScript> _modContext = new Stack<BaseScript>();

        private readonly List<PatchStateWorker> _workers = new List<PatchStateWorker>();

        private int _status = 0;

        internal BaseScript CurrentModContext => _modContext.Count == 0 ? null : _modContext.Peek();

        internal List<BaseScript> LoadedScripts { get; } = new List<BaseScript>();

        internal Texture2D MissingTex { get; private set; }

        internal ConsoleLogger Logger { get; } = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, "GrindScript");

        internal GlobalLibrary Library { get; } = new GlobalLibrary();

        internal IDAllocator Allocator { get; } = new IDAllocator();

        /// <summary>
        /// The Secrets of Grindea game instance.
        /// </summary>
        public Game1 Game { get; private set; }

        /// <summary>
        /// Supports item and equipment creation.
        /// </summary>
        public ItemModding ItemAPI { get; private set; }

        /// <summary>
        /// Supports custom music and redirects.
        /// </summary>
        public AudioModding AudioAPI { get; private set; }

        /// <summary>
        /// Supports custom level and world region creation.
        /// </summary>
        public LevelModding LevelAPI { get; private set; }

        /// <summary>
        /// Allows definition of custom commands.
        /// </summary>
        public MiscModding MiscAPI { get; private set; }

        /// <summary>
        /// Supports custom perks, treats and curses.
        /// </summary>
        public RoguelikeModding RoguelikeAPI { get; private set; }

        /// <summary>
        /// Has limited support for custom miscellaneous text.
        /// </summary>
        public TextModding TextAPI { get; private set; }

        /// <summary>
        /// Currently offers no mod functionality.
        /// </summary>
        public SaveModding SaveAPI { get; private set; }

        /// <summary>
        /// Instantiates GrindScript and sets some ModGlobals variables.
        /// </summary>
        public GrindScript()
        {
            APIGlobals.API = this;
            APIGlobals.Logger = Logger;
            Logger.Debug("GrindScript instantiated!");

            ItemAPI = new ItemModding(this);
            AudioAPI = new AudioModding(this);
            LevelAPI = new LevelModding(this);
            MiscAPI = new MiscModding(this);
            RoguelikeAPI = new RoguelikeModding(this);
            TextAPI = new TextModding(this);
            SaveAPI = new SaveModding(this);

            Library.Commands[GrindScriptCommands.APIName] = GrindScriptCommands.SetupAndGet(this);
        }

        /// <summary>
        /// Sets the given mod as the current load context, and calls the given method.
        /// GrindScript uses this method when calling each mod's LoadContent() method.
        /// </summary>
        internal void CallWithContext(BaseScript mod, Action<BaseScript> call)
        {
            _modContext.Push(mod);
            call?.Invoke(mod);
            _modContext.Pop();
        }

        /// <summary>
        /// Retrieves a PatchStateWorker of the given subtype if it exists.
        /// These are used to store state and execute code that is used in very few places
        /// </summary>
        internal T GetWorker<T>() where T : PatchStateWorker
        {
            foreach (var worker in _workers)
            {
                if (worker.GetType() == typeof(T))
                    return worker as T;
            }
            return null;
        }

        /// <summary>
        /// Retrieves a PatchStateWorker of the given subtype if it exists.
        /// These are used to store state and execute code that is used in very few places
        /// </summary>
        internal T GetNamedWorker<T>(string id) where T : PatchStateWorker
        {
            foreach (var worker in _workers)
            {
                if (worker.GetType() == typeof(T) && worker.ID == id)
                    return worker as T;
            }
            return null;
        }

        /// <summary>
        /// Prepares GrindScript by doing some processing before SoG's Main method runs.
        /// This should only be called by the Launcher
        /// </summary>
        public void SetupGrindScript()
        {
            if (_status != 0)
            {
                Logger.Error($"Can't setup self: Status is {_status}!");
                return;
            }

            Logger.Info("Setting up Grindscript...");

            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Secrets Of Grindea") == null)
            {
                Logger.Warn("Couldn't find Secrets of Grindea.exe in current AppDomain!");
                return;
            }

            Utils.Tools.TryCreateDirectory("Mods");
            Utils.Tools.TryCreateDirectory("Content/ModContent");

            SetupPatchStateWorkers();
            ApplyPatches();

            _status = 1;
        }

        /// <summary>
        /// Prepares Secrets of Grindea during its startup thread.
        /// </summary>
        internal void SetupSoG()
        {
            if (_status != 1)
            {
                Logger.Error($"Can't setup SoG: Status is {_status}!");  
                return;
            }

            Logger.Info("Setting up Secrets of Grindea..."); 

            Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");

            APIGlobals.Game = Game = (Game1)gameAssembly.GetType("SoG.Program").GetField("game").GetValue(null);

            // Place mod saves in a separate folder. This prevents crashes from damaging vanilla saves
            Game.sAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GrindScript/";

            // Disable submission of high scores for obvious reasons
            // This doesn't prevent cheating, but it avoids accidental cheater tagging by the game
            Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;

            // Load a texture that can be used as an alternate "Null Texture" when TryLoad-ing
            MissingTex = Utils.Tools.TryLoadTex("ModContent/GrindScript/NullTexGS", Game.Content);
            
            // Instantiate all mods found in "Mods/"
            LoadMods();

            _status = 2;
        }

        /// <summary>
        /// Applies all patches found in PatchCodex.
        /// </summary>
        private void ApplyPatches()
        {
            Logger.Info("Applying Patches...");

            Array allPatches = Enum.GetValues(typeof(PatchCodex.PatchID));
            var nullPatches = new List<PatchCodex.PatchID>();

            const int division = 100 / 5;
            int successCount = 0;
            int progress = 0;
            int nextProgressUpdate = division;

            foreach (PatchCodex.PatchID id in allPatches)
            {
                PatchCodex.PatchInfo patch = PatchCodex.GetPatch(id);
                progress++;

                if (patch == null)
                {
                    nullPatches.Add(id);
                    continue;
                }

                try
                {
                    if (patch.Target == null || (patch.Prefix == null && patch.Postfix == null && patch.Transpiler == null))
                        Logger.Warn($"Patch {id} may be malformed!");

                    _harmony.Patch(patch);
                    successCount++;

                    if (progress * 100 / allPatches.Length >= nextProgressUpdate)
                    {
                        Logger.Info($"{nextProgressUpdate}%...");
                        nextProgressUpdate += division;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Patch {id} threw an exception! Message: {e.Message}");
                }
            }
            
            int nullCount = nullPatches.Count;
            if (nullCount > 0)
            {
                Logger.Info($"{nullCount} null patches were encountered:");

                const int toDisplay = 3;
                int index = -1;

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
        private void LoadMod(string path)
        {
            Logger.Info("Loading mod " + Tools.GetShortenedPath(path));

            try
            {
                Type type = Assembly.LoadFile(path).GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript mod = type.GetConstructor(new Type[] { }).Invoke(new object[] { }) as BaseScript;

                mod.ModAPI = this;
                mod.ModIndex = Allocator.ModIndexID.Allocate();

                mod.ModContent = new ContentManager(Game.Content.ServiceProvider, Game.Content.RootDirectory);
                mod.ModPath = $"ModContent/{type.Name}/";

                Library.Audio.Add(mod.ModIndex, new ModAudioEntry(mod, mod.ModIndex));
                Library.Commands[mod.GetType().Name] = new Dictionary<string, CommandParser>();

                LoadedScripts.Add(mod);

                Logger.Info($"ModPath set as {mod.ModPath}");
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load mod {Tools.GetShortenedPath(path)}. Exception message: {Tools.GetShortenedPath(e.Message)}");
            }
        }

        /// <summary>
        /// Loads all mods found in the "/Mods" directory
        /// </summary>
        private void LoadMods()
        {
            var dir = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\Mods");

            foreach (var file in Directory.GetFiles(dir))
            {
                LoadMod(file);
            }
        }

        /// <summary>
        /// Instantiates workers used elsewhere.
        /// </summary>
        private void SetupPatchStateWorkers()
        {
            _workers.Add(new TCMenuWorker(this, "TC1"));
        }
    }
}
