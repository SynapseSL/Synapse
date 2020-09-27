using Synapse.Api;
using System;

namespace Synapse.Command
{
    public interface ICommand
    {
        bool Execute(ArraySegment<string> arguments, Player player,Platform platform, out string Response);

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
