using System.ComponentModel;

namespace Synapse.Config
{
    public class SynapseConfiguration : AbstractConfigSection
    {
        [Description("Enables or disables the embedded Database. Warning: Disabling this option can break plugins and is not recommended")]
        public bool DatabaseEnabled = true;
    }
}