using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;
using SoG.Modding.Utils;

namespace SoG.Modding.Content
{
    public class MiscModding : ModdingLogic
    {
        public MiscModding(GrindScript modAPI)
            : base(modAPI) { }

        /// <summary>
        /// Helper method for setting up multiple commands.
        /// </summary>
        public void CreateCommand(IDictionary<string, CommandParser> parsers)
        {
            foreach (var kvp in parsers)
                CreateCommand(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Adds a new command that executes the given parser when called.
        /// The command can be executed by typing in chat "/(ModName):(command) (argList)". <para/>
        /// The command must not have whitespace in it.
        /// </summary>
        public void CreateCommand(string command, CommandParser parser)
        {
            ThrowHelper.ThrowIfNull(command, parser);

            BaseScript mod = _modAPI.CurrentModContext;

            if (mod == null)
            {
                _modAPI.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateCommand));
                return;
            }

            if (command.Any(char.IsWhiteSpace))
                throw new ArgumentException("Provided command contains whitespace.");

            string name = mod.GetType().Name;

            if (!_modAPI.Library.Commands.TryGetValue(name, out var parsers))
            {
                _modAPI.Logger.Error($"Couldn't retrieve command table for mod {name}!");
                return;
            }

            if (parser == null)
            {
                parsers.Remove(command);
                _modAPI.Logger.Info($"Cleared command /{name}:{command}.");
            }
            else
            {
                parsers[command] = parser;
                _modAPI.Logger.Info($"Updated command /{name}:{command}.");
            }
        }
    }
}
