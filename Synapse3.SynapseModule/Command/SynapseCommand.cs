using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;

namespace Synapse3.SynapseModule.Command;

/// <summary>
/// The Default Class that Commands inherit to become hooked by the Neuron Command system
/// </summary>
public abstract class SynapseCommand : Command<SynapseContext>
{
    /// <summary>
    /// This method is executed before Execute and is recommended to be a check for Permissions
    /// </summary>
    /// <returns>Return null for normal execution or an actual CommandResult to stop the Command execution</returns>
    public override CommandResult PreExecute(SynapseContext context)
    {
        if (context.IsAdmin) return null;
        
        if (Meta is not SynapseCommandAttribute attribute)
            return new CommandResult()
            {
                Response = "Invalid Command, cant check for Permissions",
                StatusCode = CommandStatusCode.Error
            };

        if (attribute.Permission != "")
        {
            if (!context.Player.HasPermission(attribute.Permission))
            {
                return new CommandResult()
                {
                    Response = $"You don't have access to this Command ({attribute.Permission})",
                    StatusCode = CommandStatusCode.Forbidden
                };
            }
        }
        return null;
    }
}