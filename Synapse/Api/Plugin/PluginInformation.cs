using System;

namespace Synapse.Api.Plugin
{
    public class PluginInformation : Attribute
    {
        public int SynapseMajor = SynapseController.SynapseMajor;
        public int SynapseMinor = SynapseController.SynapseMinor;
        public int SynapsePatch = SynapseController.SynapsePatch;
        public string Name = "Unknown";
        public string Author = "Unknown";
        public string Description = "Unknown";
        public string Version = "Unknown";
        public int LoadPriority = int.MinValue;

        internal bool shared = false;
    }
}
