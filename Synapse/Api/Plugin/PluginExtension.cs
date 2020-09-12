using Synapse.Config;

namespace Synapse.Api.Plugin
{
    public class PluginExtension
    {
        internal PluginExtension(PluginInformations informations)
        {
            Informations = informations;
            Translation = new Translation(Informations.Name);
        }

        public PluginInformations Informations { get; private set; }

        public Translation Translation { get; }

        public ConfigHandler Config => SynapseController.Server.Configs;
    }
}
