using System;
using System.Reflection;
using Synapse.Api;

namespace Synapse.Command
{
    public class GeneratedCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, Player player, out string Response)
        {
            var command = new CommandContext()
            {
                Arguments = arguments,
                Player = player
            };
            var result = OnCommand.Invoke(command);
            Response = result.Message;
            return result.State == CommandResultState.Ok;
        }

        public Func<CommandContext,CommandResult> OnCommand { get; set; }
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public string Permission { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
        
        public Platform[] Platforms { get; set; }

        public static GeneratedCommand FromSynapseCommand(ISynapseCommand command)
        {
            var type = command.GetType();
            var cmdInf = type.GetCustomAttribute<CommandInformations>();
            return new GeneratedCommand
            {
                OnCommand = command.Execute,
                Name = cmdInf.Name,
                Aliases = cmdInf.Aliases,
                Permission = cmdInf.Permission??"",
                Usage = cmdInf.Usage,
                Description = cmdInf.Description??"",
                Platforms = cmdInf.Platforms??new []{Platform.RemoteAdmin, Platform.ServerConsole}
            };
        }
    }
    
    public class CommandContext
    {
        public ArraySegment<string> Arguments;
        public Player Player;
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