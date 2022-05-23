using System;
using System.Reflection;
using Synapse.Api;

namespace Synapse.Command
{
    public class GeneratedCommand : ICommand
    {
        public CommandResult Execute(CommandContext command)
        {
            return OnCommand.Invoke(command);
        }

        public Func<CommandContext, CommandResult> OnCommand { get; set; }
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public string Permission { get; set; }
        public string Usage { get; set; }
        public string[] Arguments { get; set; }
        public string Description { get; set; }

        public Platform[] Platforms { get; set; }

        public static GeneratedCommand FromSynapseCommand(ISynapseCommand command)
        {
            var type = command.GetType();
            var cmdInf = type.GetCustomAttribute<CommandInformation>();
            return new GeneratedCommand
            {
                OnCommand = command.Execute,
                Name = cmdInf.Name,
                Aliases = cmdInf.Aliases,
                Permission = cmdInf.Permission ?? "",
                Usage = cmdInf.Usage,
                Arguments = cmdInf.Arguments,
                Description = cmdInf.Description ?? "",
                Platforms = cmdInf.Platforms ?? new[] { Platform.RemoteAdmin, Platform.ServerConsole }
            };
        }
    }

    public class CommandContext
    {
        public ArraySegment<string> Arguments;
        public Player Player;
        public Platform Platform;
    }

    public class CommandResult
    {
        public CommandResultState State;
        public string Message;
    }

    public enum CommandResultState
    {
        Ok,
        Error,
        NoPermission
    }
}