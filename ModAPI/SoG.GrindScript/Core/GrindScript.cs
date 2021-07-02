using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Core;
using SoG.Modding.Content;
using SoG.Modding.Extensions;
using SoG.Modding.Tools;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Core of the API
    /// </summary>

    public class GrindScript
    {
        private readonly Harmony _harmony = new Harmony("GrindScriptPatcher");

        private readonly Stack<BaseScript> _modContext = new Stack<BaseScript>();

        private int _status = 0;

        internal BaseScript CurrentModContext => _modContext.Count == 0 ? null : _modContext.Peek();

        internal List<BaseScript> LoadedScripts { get; } = new List<BaseScript>();

        internal Texture2D MissingTex { get; private set; }

        internal ConsoleLogger Logger { get; private set; } = new ConsoleLogger(ConsoleLogger.LogLevels.Debug, "GrindScript");

        internal GlobalLibrary Library = new GlobalLibrary();

        internal IDAllocator Allocator = new IDAllocator();

        public ItemModding ItemAPI { get; private set; }

        public AudioModding AudioAPI { get; private set; }

        public LevelModding LevelAPI { get; private set; }

        public MiscModding MiscAPI { get; private set; } // Misc sounds like multiple responsibility, therefore bad?

        public RoguelikeModding RoguelikeAPI { get; private set; }

        public TextModding TextAPI { get; private set; }

        public SaveModding SaveAPI { get; private set; }

        public GrindScript()
        {
            ModGlobals.API = this;
            ModGlobals.Log = Logger;
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
        /// Sets a load context. All subsequent content creation methods will use the given context as the target. <para/>
        /// BaseScript::LoadContent() is called with the current mod as context. <para/>
        /// Only use this if you really need to load on behalf of other mods, or outside of LoadContent()
        /// </summary>

        internal void CallWithContext(BaseScript mod, Action<BaseScript> call)
        {
            _modContext.Push(mod);
            call?.Invoke(mod);
            _modContext.Pop();
        }

        /// <summary>
        /// Prepares GrindScript by doing some processing before SoG's Main method runs.
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

            Utils.TryCreateDirectory("Mods");
            Utils.TryCreateDirectory("Content/ModContent");

            ApplyPatches();

            _status = 1;
        }

        /// <summary>
        /// Initializes GrindScript during SoG's startup thread
        /// </summary>

        public void SetupSoG()
        {
            if (_status != 1)
            {
                Logger.Error($"Can't setup SoG: Status is {_status}!");  
                return;
            }

            Logger.Info("Setting up Secrets of Grindea..."); 

            Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea");

            ModGlobals.Game = (Game1)gameAssembly.GetType("SoG.Program").GetField("game").GetValue(null);

            // Place mod saves in a separate folder. This prevents crashes from damaging vanilla saves
            ModGlobals.Game.sAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/GrindScript/";

            // Disable submission of high scores for obvious reasons
            // This doesn't prevent cheating, but it avoids accidental cheater tagging by the game
            ModGlobals.Game.xGameSessionData.xRogueLikeSession.bTemporaryHighScoreBlock = true;

            // Load a texture that can be used as an alternate "Null Texture" when TryLoad-ing
            MissingTex = Utils.TryLoadTex("ModContent/GrindScript/NullTexGS", ModGlobals.Game.Content);
            
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
            Logger.Info("Loading mod " + path);

            try
            {
                Type type = Assembly.LoadFile(path).GetTypes().First(t => t.BaseType == typeof(BaseScript));
                BaseScript mod = type.GetConstructor(new Type[] { }).Invoke(new object[] { }) as BaseScript;

                mod.ModAPI = this;

                mod.ModIndex = Allocator.ModIndexID.Allocate();

                Library.Audio.Add(mod.ModIndex, new ModAudioEntry(mod, mod.ModIndex));

                Library.Commands[mod.GetType().Name] = new Dictionary<string, CommandParser>();

                LoadedScripts.Add(mod);

                Logger.Info("Loaded mod " + path);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load mod {path}. Exception message: {e.Message}");
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
    }
}
