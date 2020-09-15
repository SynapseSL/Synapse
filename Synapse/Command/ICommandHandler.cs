using System;
using System.Collections.Generic;

namespace Synapse.Command
{
    public interface ICommandHandler
    {
        List<ICommand> Commands { get; }

        bool TryGetCommand(string name, out ICommand cmd);

        bool RegisterCommand(ICommand command);

        event Action<ICommandHandler> ReloadCommandHandlerEvent;
    }
}
