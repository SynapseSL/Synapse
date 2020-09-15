using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Synapse.Command
{
    public class CommandHandler : ICommandHandler
    {
        private readonly Dictionary<string, string> commandAliases = new Dictionary<string, string>();

        private readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        internal CommandHandler() => Reload();

        public List<ICommand> Commands { get; }

        public bool TryGetCommand(string name, out ICommand cmd)
        {
            if (commandAliases.TryGetValue(name, out var alias))
                name = alias;

            return commands.TryGetValue(name, out cmd);
        }

        public bool RegisterCommand(ICommand command)
        {
            var infos = command.GetType().GetCustomAttribute<CommandInformations>();

            if (string.IsNullOrWhiteSpace(infos.Name))
                return false;

            commands.Add(infos.Name, command);

            if (infos.Aliases != null)
                foreach (var alias in infos.Aliases)
                    if (!string.IsNullOrWhiteSpace(alias))
                        commandAliases.Add(alias, infos.Name);

            return true;
        }

        public void Reload()
        {
            commands.Clear();
            commandAliases.Clear();
            ReloadCommandHandlerEvent.Invoke(this);
        }

        public event Action<ICommandHandler> ReloadCommandHandlerEvent;
    }
}
