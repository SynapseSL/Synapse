using Neuron.Core.Dev;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseCommand(
    CommandName = "Language",
    Aliases = new[] { "lang" },
    Description = "Gives/Sets your Language that should be used for you on this Server",
    Platforms = new [] { CommandPlatform.PlayerConsole }
)]
public class LanguageCommand : SynapseCommand
{
    private SynapseConfigService _config;

    public LanguageCommand(SynapseConfigService config)
    {
        _config = config;
    }
    
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if (context.Arguments.Length == 0)
        {
            var language = context.Player.GetData("language");
            if (string.IsNullOrWhiteSpace(language))
            {
                result.Response = context.Player.GetTranslation(_config.Translation).TranslationCommandNoTranslation;
                return;
            }

            result.Response = context.Player.GetTranslation(_config.Translation).TranslationCommandGetTranslation
                .Format(language);
            return;
        }

        if (context.Player.DoNotTrack)
        {
            result.StatusCode = CommandStatusCode.Error;
            result.Response = context.Player.GetTranslation(_config.Translation).DnT;
            return;
        }
        
        context.Player.SetData("language",context.Arguments[0].ToUpper());
        result.Response = context.Player.GetTranslation(_config.Translation).TranslationCommandSetTranslation
            .Format(context.Arguments[0].ToUpper());
    }
}