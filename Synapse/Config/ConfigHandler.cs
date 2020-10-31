using System.Collections.Generic;
using Synapse.Api.Plugin;

namespace Synapse.Config
{
    public class ConfigHandler
    {
        internal ConfigHandler() { }

        internal SynapseConfiguration SynapseConfiguration;
        internal Translation SynapseTranslation;

        private SYML _syml;
        
        internal void Init()
        {
            SynapseTranslation = new Translation(new PluginInformation { Name = "Synapse" });
            var trans = new Dictionary<string, string>
            {
                {"sameteam","You can't harm this Person" },
                {"scpteam","As your current Role you can't harm an Scp"  },
            };
            SynapseTranslation.CreateTranslations(trans);

            _syml = new SYML(SynapseController.Server.Files.ConfigFile);
            _syml.Load();
            SynapseConfiguration = new SynapseConfiguration();
            SynapseConfiguration = _syml.GetOrSetDefault("Synapse", SynapseConfiguration);
        }

        public T GetOrSetDefault<T>(string section, T defValue) where T : IConfigSection
        {
            return _syml.GetOrSetDefault(section, defValue);
        }
        
        public object GetOrSetDefault(string section, object o)
        {
            return _syml.GetOrSetDefaultUnsafe(section, o);
        }
        
        public void UpdateSection<T>(string section, T replacement) where T : IConfigSection
        {
            var sec = new ConfigSection(section, "");
            if (_syml.Sections.ContainsKey(section))
            {
                sec = _syml.Sections[section];
            }

            sec.Import(replacement);
            _syml.Sections[section] = sec;
            _syml.Store();
        }
        
        public void Reload()
        {
            _syml.Load();
            SynapseTranslation.ReloadTranslations();
        }
    }
}
