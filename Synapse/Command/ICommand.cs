using Synapse.Api;
using System;
using System.Reflection;
using Synapse.Api.Plugin;

namespace Synapse.Command
{
    public interface ICommand
    {
        bool Execute(ArraySegment<string> arguments, Player player, out string Response);

        string Name
        {
            get;
            set;
        }
        
        string[] Aliases
        {
            get;
            set;
        }
        
        string Permission
        {
            get;
            set;
        }

        string Usage
        {
            get;
            set;
        }
        
        string Description
        {
            get;
            set;
        }

        Platform[] Platforms
        {
            get;
            set;
        }
    }
    

}
