using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.Command;

public abstract class SynapseCommand : Command<SynapseContext>
{
    public override CommandResult PreExecute(SynapseContext context)
    {
        if (context.IsAdmin) return null;
        
        if (Meta is not SynapseCommandAttribute attribute)
            return new CommandResult()
            {
                Response = "Invalid Command, cant check for Permissions",
                StatusCode = CommandStatusCode.Error
            };
        
        //TODO: Check Player permissions and implement Translation for Error above
        
        return null;
    }
}