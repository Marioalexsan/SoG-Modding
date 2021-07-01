using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    public static class MiscModding
    {
        /// <summary>
        /// Helper method for setting up multiple commands.
        /// </summary>

        public static void ConfigureCommandsFrom(BaseScript owner, IDictionary<string, CommandParser> parsers)
        {
            if (owner == null || parsers == null)
            {
                GrindScript.Logger.Warn("Can't configure commands due to owner or parsers being null!");
                return;
            }

            foreach (var kvp in parsers)
            {
                ConfigureCommand(owner, kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Adds a new command that can be executed by typing in chat "/(ModName):(command) (argList)" <para/>
        /// The command must not have whitespace in it.
        /// </summary>

        public static void ConfigureCommand(BaseScript owner, string command, CommandParser parser)
        {
            if (owner == null || command == null)
            {
                GrindScript.Logger.Warn("Can't configure command due to owner or command being null!");
                return;
            }

            string name = owner.GetType().Name;

            if (!ModLibrary.Commands.TryGetValue(name, out var parsers))
            {
                GrindScript.Logger.Error($"Couldn't retrieve command table for mod {name}!");
                return;
            }

            if (parser == null)
            {
                parsers.Remove(command);
                GrindScript.Logger.Info($"Cleared command /{name}:{command}.");
            }
            else
            {
                parsers[command] = parser;
                GrindScript.Logger.Info($"Updated command /{name}:{command}.");
            }
        }
    }
}
