using System;

namespace Synapse.Api.Plugin
{
    public class PluginInformations : Attribute
    {
        public int SynapseMajor;
        public int SynapseMinor;
        public int SynapsePatch;
        public string Name;
        public string Author;
        public string Description;
        public string Version;
    }
}
