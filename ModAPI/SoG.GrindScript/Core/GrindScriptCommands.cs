using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Utils;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Prepares chat commands that come bundled with GrindScript.
    /// </summary>
    internal static class GrindScriptCommands
    {
        /// <summary> 
        /// Acts as the "mod name" when using GrindScript commands 
        /// </summary>
        public const string APIName = "GrindScript";

        /// <summary>
        /// Builds and returns a dictionary of commands for the API.
        /// </summary>
        public static Dictionary<string, CommandParser> SetupAndGet(GrindScript modAPI)
        {
            return new Dictionary<string, CommandParser>
            {
                [nameof(ModList)] = ModList(modAPI),
                [nameof(Help)] = Help(modAPI),
                [nameof(PlayerPos)] = PlayerPos(modAPI),
                [nameof(ModTotals)] = ModTotals(modAPI)
            };
        }

        private static CommandParser Help(GrindScript modAPI)
        {
            return (message, _2) =>
            {
                Dictionary<string, CommandParser> commandList = null;
                var args = Utils.Tools.GetArgs(message);
                if (args.Length == 0)
                {
                    commandList = modAPI.Library.Commands[APIName];
                }
                else if (!modAPI.Library.Commands.TryGetValue(args[0], out commandList))
                {
                    CAS.AddChatMessage($"[{APIName}] Unknown mod!");
                    return;
                }
                CAS.AddChatMessage($"[{APIName}] Command list{(args.Length == 0 ? "" : $" for {args[0]}")}:");

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

        private static CommandParser ModList(GrindScript modAPI)
        {
            return (_1, _2) =>
            {
                CAS.AddChatMessage($"[GrindScript] Mod Count: {modAPI.LoadedScripts.Count}");

                var messages = new List<string>();
                var concated = "";
                foreach (var mod in modAPI.LoadedScripts)
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

        private static CommandParser PlayerPos(GrindScript modAPI)
        {
            return (_1, _2) =>
            {
                var local = modAPI.Game.xLocalPlayer.xEntity.xTransform.v2Pos;

                CAS.AddChatMessage($"[{APIName}] Player position: {(int)local.X}, {(int)local.Y}");
            };
        }

        private static CommandParser ModTotals(GrindScript modAPI)
        {
            return (message, _2) =>
            {
                var args = Utils.Tools.GetArgs(message);
                if (args.Length != 1)
                {
                    CAS.AddChatMessage($"[{APIName}] Usage: /GrindScript:ModTotals <unique type>");
                    return;
                }

                switch (args[0])
                {
                    case "Items":
                        CAS.AddChatMessage($"[{APIName}] Items defined: " + modAPI.Library.Items.Count);
                        break;
                    case "Perks":
                        CAS.AddChatMessage($"[{APIName}] Perks defined: " + modAPI.Library.Perks.Count);
                        break;
                    case "Treats":
                    case "Curses":
                        CAS.AddChatMessage($"[{APIName}] Treats and Curses defined: " + modAPI.Library.TreatsCurses.Count);
                        break;
                    default:
                        CAS.AddChatMessage($"[{APIName}] Usage: /GrindScript:ModTotals <unique type>");
                        break;
                }
            };
        }
    }
}
