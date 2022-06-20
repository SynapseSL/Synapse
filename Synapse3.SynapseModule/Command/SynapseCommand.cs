﻿using Neuron.Modules.Commands;
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

        if (attribute.Permission != "")
        {
            if (!context.Player.HasPermission(attribute.Permission))
            {
                return new CommandResult()
                {
                    Response = "You don't have access to this Command",
                    StatusCode = CommandStatusCode.Forbidden
                };
            }
        }
        return null;
    }
}