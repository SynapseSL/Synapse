namespace Synapse.Command
{
    public class Handlers
    {
        internal Handlers() { }

        public CommandHandler RemoteAdminHandler { get; } = new CommandHandler();

        public CommandHandler ClientCommandHandler { get; } = new CommandHandler();

        public CommandHandler ServerConsoleHandler { get; } = new CommandHandler();


        internal static void RegisterCommand(ISynapseCommand iSynapseCommand)
        {
            var command = GeneratedCommand.FromSynapseCommand(iSynapseCommand);
            foreach (var platform in command.Platforms)
            {
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
        }
    }
}
