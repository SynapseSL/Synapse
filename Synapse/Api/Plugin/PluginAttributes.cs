using System;

namespace Synapse.Api.Plugin
{
    public class Config : Attribute
    {
        public string section = null;
        public int revision = 0;
    }

    public class SynapseTranslation : Attribute { }
}