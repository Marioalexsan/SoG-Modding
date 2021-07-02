using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    public class MiscModding : ModdingLogic
    {
        public MiscModding(GrindScript modAPI)
            : base(modAPI) { }

        /// <summary>
        /// Helper method for setting up multiple commands.
        /// </summary>

        public void ConfigureCommands(IDictionary<string, CommandParser> parsers)
        {
            foreach (var kvp in parsers)
                ConfigureCommand(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Adds a new command that can be executed by typing in chat "/(ModName):(command) (argList)" <para/>
        /// The command must not have whitespace in it.
        /// </summary>

        public void ConfigureCommand(string command, CommandParser parser)
        {
            BaseScript owner = _modAPI.CurrentModContext;
            if (owner == null || command == null)
            {
                ModGlobals.Log.Warn("Can't configure command due to owner or command being null!");
                return;
            }

            string name = owner.GetType().Name;

            if (!_modAPI.Library.Commands.TryGetValue(name, out var parsers))
            {
                ModGlobals.Log.Error($"Couldn't retrieve command table for mod {name}!");
                return;
            }

            if (parser == null)
            {
                parsers.Remove(command);
                ModGlobals.Log.Info($"Cleared command /{name}:{command}.");
            }
            else
            {
                parsers[command] = parser;
                ModGlobals.Log.Info($"Updated command /{name}:{command}.");
            }
        }
    }
}
