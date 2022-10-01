using System.Collections.Generic;
using System.Linq;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using RemoteAdmin;
using Synapse3.SynapseModule.Config;
using Synapse3.SynapseModule.Player;
using ICommand = Neuron.Modules.Commands.Command.ICommand;

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
    private readonly SynapseConfigService _configService;

    public HelpCommand(SynapseConfigService configService)
    {
        _configService = configService;
    }
    
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        List<ICommand> commandlist = new List<ICommand>();
        IEnumerable<CommandSystem.ICommand> vanilla = new List<CommandSystem.ICommand>();

        switch (context.Platform)
        {
            case CommandPlatform.PlayerConsole:
                commandlist = Synapse.Get<SynapseCommandService>().PlayerConsole.Handler.Commands.ToList();
                vanilla = QueryProcessor.DotCommandHandler.AllCommands;
                break;

            case CommandPlatform.RemoteAdmin:
                commandlist = Synapse.Get<SynapseCommandService>().RemoteAdmin.Handler.Commands.ToList();
                vanilla = GameCore.Console.singleton.ConsoleCommandHandler.AllCommands;
                break;

            case CommandPlatform.ServerConsole:
                commandlist = Synapse.Get<SynapseCommandService>().ServerConsole.Handler.Commands.ToList();
                vanilla = CommandProcessor.RemoteAdminCommandHandler.AllCommands;
                break;

            default:
                result.Response = "Information for this Console is not supported";
                result.StatusCode = CommandStatusCode.Error;
                return;
        }
        

            if (context.Arguments.Length > 0 && !string.IsNullOrWhiteSpace(context.Arguments.First()))
            {
                //Single Command Info
                return;
            }

            result.Response = GenerateCommandList(commandlist, vanilla, context.Player, context.Platform);
            result.StatusCode = CommandStatusCode.Ok;
    }

    public string GenerateCommandList(List<ICommand> customCommands,
        IEnumerable<CommandSystem.ICommand> vanillaCommands, SynapsePlayer player,CommandPlatform platform)
    {
        var msg = _configService.Translation.Get(player).CommandHelp;
        foreach (var command in vanillaCommands)
        {
            msg += $"\n{command.Command}";

            if ((command.Aliases?.Length ?? 0) > 0)
            {
                msg += $"\n    Aliases: " + string.Join(", ", command.Aliases);
            }

            if (!string.IsNullOrWhiteSpace(command.Description))
            {
                if (command.Description.Length <= _maxLetters[platform])
                {
                    msg += "\n    Description: " + command.Description;
                }
                else
                {
                    msg += "\n    Description: " + SplitDescription(command.Description, platform);
                }
            }

            msg += "\n";
        }

        msg += "\n" + _configService.Translation.Get(player).CommandHelpSecond;

        foreach (var customCommand in customCommands)
        {
            if (customCommand.Meta is SynapseCommandAttribute synapseCommandAttribute)
            {
                if (!string.IsNullOrWhiteSpace(synapseCommandAttribute.Permission) &&
                    player.HasPermission(synapseCommandAttribute.Permission)) continue;
            }

            msg += $"\n{customCommand.Meta.CommandName}";
            if ((customCommand.Meta.Aliases?.Length ?? 0) > 0)
            {
                msg += $"\n    Aliases: " + string.Join(", ", customCommand.Meta.Aliases);
            }

            if (!string.IsNullOrWhiteSpace(customCommand.Meta.Description))
            {
                if (customCommand.Meta.Description.Length <= _maxLetters[platform])
                {
                    msg += "\n    Description: " + customCommand.Meta.Description;
                }
                else
                {
                    msg += "\n    Description: " + SplitDescription(customCommand.Meta.Description, platform);
                }
            }
            
            msg += "\n";
        }

        return msg.TrimEnd('\n');
    }

    public string GenerateCustomCommandInfo(ICommand command)
    {
        return "";
    }

    public string GenerateVanillaCommandInfo(CommandSystem.ICommand command)
    {
        return "";
    }

    private string SplitDescription(string message, CommandPlatform platform)
    {
        var count = 0;
        var msg = "";

        foreach (var word in message.Split(' '))
        {
            count += word.Length;

            if (count > _maxLetters[platform])
            {
                msg += "\n                ";
                count = 0;
            }

            if (msg == string.Empty)
            {
                msg += word;
            }
            else
            {
                msg += " " + word;
            }
        }

        return msg;
    }

    private readonly Dictionary<CommandPlatform, int> _maxLetters = new()
    {
        { CommandPlatform.PlayerConsole, 50 },
        { CommandPlatform.RemoteAdmin, 50 },
        { CommandPlatform.ServerConsole, 75 }
    };
}