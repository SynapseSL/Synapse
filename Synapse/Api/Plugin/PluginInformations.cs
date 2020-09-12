using System;

namespace Synapse.Api.Plugin
{
    public class PluginInformations : Attribute
    {
        public int SynapseMajor = 2;
        public int SynapseMinor = 0;
        public int SynapsePatch = 0;
        public string Name = "Unknown";
        public string Author = "Unknown";
        public string Description = "Unknown";
        public string Version = "Unknown";
        public int LoadPriority = int.MaxValue;
    }
}
