using System;
using System.Reflection;
using Synapse.Command;

namespace Synapse.Api.Plugin.Processors
{
    public class CommandProcessor : IContextProcessor
    {
        public void Process(PluginLoadContext context)
        {
            foreach (var @class in context.Classes)
            {
                if (!typeof(ISynapseCommand).IsAssignableFrom(@class)) continue;
                var inf = @class.GetCustomAttribute<CommandInformations>();
                if (inf == null) continue;
                var classObject = Activator.CreateInstance(@class);
                var command = GeneratedCommand.FromSynapseCommand(classObject as ISynapseCommand);
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
}