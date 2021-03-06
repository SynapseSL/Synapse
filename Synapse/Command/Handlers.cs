using System.Collections.Generic;
using Synapse.Command.Commands;

namespace Synapse.Command
{
    public class Handlers
    {
        private static readonly List<ISynapseCommand> AwaitingFinalization = new List<ISynapseCommand>();

        internal Handlers()
        {
        }

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
            RegisterCommand(new SynapseNetworkSyncCommand(), false);
            RegisterCommand(new SynapseGccCommand(), false);
        }

        internal static void RegisterCommand(ISynapseCommand iSynapseCommand, bool awaitPluginInitialisation)
        {
            if (awaitPluginInitialisation)
            {
                AwaitingFinalization.Add(iSynapseCommand);
                return;
            }

            var command = GeneratedCommand.FromSynapseCommand(iSynapseCommand);
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

        internal static void FinalizePluginsCommands()
        {
            foreach (var iSynapseCommand in AwaitingFinalization)
            {
                var command = GeneratedCommand.FromSynapseCommand(iSynapseCommand);
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

            AwaitingFinalization.Clear();
        }
    }
}