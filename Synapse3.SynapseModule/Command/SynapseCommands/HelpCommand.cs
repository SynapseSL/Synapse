using System.Collections.Generic;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using System.Linq;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "help",
    Aliases = new[] { "h" },
    Parameters = new[] { "(Command)" },
    Description = "Shows all available commands with usage description",
    Permission = "synapse.command.help",
    Platforms = new [] { CommandPlatform.PlayerConsole, CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
    )]
public class HelpCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        List<ICommand> commandlist = new List<ICommand>();

            switch (context.Platform)
            {
                case CommandPlatform.PlayerConsole:
                    commandlist = Synapse.Get<SynapseCommandService>().PlayerConsole.Handler.Commands.ToList();
                    break;

                case CommandPlatform.RemoteAdmin:
                    commandlist = Synapse.Get<SynapseCommandService>().RemoteAdmin.Handler.Commands.ToList();
                    break;

                case CommandPlatform.ServerConsole:
                    commandlist = Synapse.Get<SynapseCommandService>().ServerConsole.Handler.Commands.ToList();
                    break;

                default:
                    result.Response = "Information for this Console is not supported";
                    result.StatusCode = CommandStatusCode.Error;
                    return;
            }

            commandlist = commandlist.Where(x => context.Player.HasPermission((x.Meta as SynapseCommandAttribute)?.Permission) || string.IsNullOrWhiteSpace((x.Meta as SynapseCommandAttribute)?.Permission) || (x.Meta as SynapseCommandAttribute)?.Permission.ToUpper() == "NONE").ToList();

            if (context.Arguments.Length > 0 && !string.IsNullOrWhiteSpace(context.Arguments.First()))
            {
                var command = commandlist.FirstOrDefault(x => (x.Meta as SynapseCommandAttribute)?.CommandName.ToLower() == context.Arguments.First());

                if (command == null)
                {
                    foreach (ICommand c in commandlist.Where(c => (c.Meta as SynapseCommandAttribute)?.Aliases.FirstOrDefault(i => i.ToLower() == context.Arguments.First()) != null))
                    {
                        command = c;
                    }

                    if (command == null)
                    {
                        result.StatusCode = CommandStatusCode.Error;
                        result.Response = "No Command with this Name found";
                        return;
                    }
                }


                if (command.Meta is SynapseCommandAttribute commandAttribute)
                {
                    var commandPlatforms = commandAttribute.Platforms;
                    if (commandPlatforms != null)
                    {
                        string platforms = "{ " + string.Join(", ", commandPlatforms) + " }";
                        
                        var commandAliases = commandAttribute.Aliases;
                        if (commandAliases != null)
                        {
                            string aliases = "{ " + string.Join(", ", commandAliases) + " }";

                            if (string.IsNullOrWhiteSpace(commandAttribute?.Permission))
                                result.Response = $"\n{commandAttribute.CommandName}\n    - Description: {commandAttribute.Description}\n    - Platforms: {platforms}\n    - Aliases: {aliases}";
                            else
                                result.Response = $"\n{commandAttribute.CommandName}\n    - Permission: {commandAttribute.Permission}\n    - Description: {commandAttribute.Description}\n    - Platforms: {platforms}\n    - Aliases: {aliases}";
                        }
                    }
                }
                

                result.StatusCode = CommandStatusCode.Ok;
                return;
            }

            var msg = $"All Commands which you can execute for {context.Platform}:";

            foreach (var command in commandlist)
            {
                SynapseCommandAttribute commandAttribute = command.Meta as SynapseCommandAttribute;
                if (commandAttribute?.Aliases != null)
                {
                    string alias = "{ " + string.Join(", ", commandAttribute.Aliases) + " }";

                    msg += $"\n{commandAttribute.CommandName}:\n    -Description: {commandAttribute.Description}\n    -Aliases: {alias}";
                }
            }

            result.Response = msg;
            result.StatusCode = CommandStatusCode.Ok;
    }
}