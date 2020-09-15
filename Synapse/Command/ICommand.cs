using Synapse.Api;
using System;

namespace Synapse.Command
{
    public interface ICommand
    {
        bool Execute(ArraySegment<string> arguments, Player player, out string Response);
    }
}
