using System;

namespace Synapse.Command
{
    public class CommandInformations : Attribute
    {
        public string Name;

        public string[] Aliases;

        public string Permission;

        public string Usage;

        public string Description;

        public Platform[] Platforms;
    }

    public enum Platform
    {
        RemoteAdmin,
        ClientConsole,
        ServerConsole
    }
}
