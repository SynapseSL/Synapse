namespace Synapse.Command
{
    public interface ISynapseCommand
    {
        CommandResult Execute(CommandContext context);
    }
}