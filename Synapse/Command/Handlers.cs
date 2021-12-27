using System.Collections.Generic;
using Synapse.Command.Commands;
using System.Linq;

namespace Synapse.Command
{
    public class Handlers
    {
        private static readonly List<ISynapseCommand> AwaitingFinalization = new List<ISynapseCommand>();

        internal Handlers() { }

        public CommandHandler RemoteAdminHandler { get; } = new CommandHandler();

        public CommandHandler ClientCommandHandler { get; } = new CommandHandler();

        public CommandHandler ServerConsoleHandler { get; } = new CommandHandler();


        internal void RegisterSynapseCommands()
        {
            RegisterCommand(new SynapseHelpCommand(), false);
            RegisterCommand(new SynapseReloadCommand(), false);
            RegisterCommand(new SynapseKeyPressCommand(), false);
            RegisterCommand(new SynapsePluginCommand(), false);
            RegisterCommand(new SynapsePermissionCommand(), false);
            RegisterCommand(new SynapseGiveCustomItemCommand(), false);
            RegisterCommand(new SynapseSetClassCommand(), false);
            RegisterCommand(new SynapseMapPointCommand(), false);
            RegisterCommand(new SynapseRespawnCommand(), false);
        }

        internal static void RegisterCommand(ISynapseCommand iSynapseCommand, bool awaitPluginInitialisation)
        {
            if (awaitPluginInitialisation)
            {
                AwaitingFinalization.Add(iSynapseCommand);
                return;
            }

            RegisterGeneratedCommand(GeneratedCommand.FromSynapseCommand(iSynapseCommand));
        }

        internal static void FinalizePluginsCommands()
        {
            foreach (var iSynapseCommand in AwaitingFinalization)
                RegisterGeneratedCommand(GeneratedCommand.FromSynapseCommand(iSynapseCommand));

            AwaitingFinalization.Clear();
        }

        internal static void RegisterGeneratedCommand(GeneratedCommand command)
        {
            foreach (var platform in command.Platforms)
                switch (platform)
                {
                    case Platform.ClientConsole:

                        SynapseController.CommandHandlers.ClientCommandHandler.RegisterCommand(command);
                        break;
                    case Platform.RemoteAdmin:
                        SynapseController.CommandHandlers.RemoteAdminHandler.RegisterCommand(command);
                        break;
                    case Platform.ServerConsole:
                        SynapseController.CommandHandlers.ServerConsoleHandler.RegisterCommand(command);
                        break;
                }
        }

        internal void GenerateCommandCompletion()
        {
            var list = RemoteAdmin.QueryProcessor._commands.ToList();
            foreach(var command in RemoteAdminHandler.Commands)
            {
                list.Add(new RemoteAdmin.QueryProcessor.CommandData
                {
                    Command = command.Name,
                    AliasOf = null,
                    Description = command.Description,
                    Hidden = false,
                    Usage = command.Arguments
                });

                if (command.Aliases == null) continue;
                
                foreach (var ali in command.Aliases)
                    list.Add(new RemoteAdmin.QueryProcessor.CommandData
                    {
                        Command = ali,
                        AliasOf = command.Name,
                        Description = command.Description,
                        Hidden = false,
                        Usage = command.Arguments
                    });
            }

            RemoteAdmin.QueryProcessor._commands = list.ToArray();
        }
    }
}