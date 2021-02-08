using Synapse.Translation;
using System.IO;

namespace Synapse.Config
{
    public class ConfigHandler
    {
        internal ConfigHandler() { }

        internal SynapseConfiguration synapseConfiguration;
        internal SynapseTranslation<Translation.Translation> synapseTranslation;

        private SYML _syml;
        
        internal void Init()
        {
            synapseTranslation = new SynapseTranslation<Translation.Translation>(Server.Get.Files.GetTranslationPath("Synapse"));
            synapseTranslation.AddTranslation(new Synapse.Translation.Translation(), "ENGLISH");
            synapseTranslation.AddTranslation(new Translation.Translation
            {
                noPermissions = "Du hast keine Berechtigung diesen Command zu benutzen!(%perm%)",
                sameTeam = "<b>Du kannst diese Person nicht verletzen!</b>",
                scpTeam = "Als deine jetzige Rolle kannst du nichts machen was ein SCP verletzen würde!",
            }, "GERMAN");

            _syml = new SYML(SynapseController.Server.Files.ConfigFile);
            _syml.Load();
            synapseConfiguration = new SynapseConfiguration();
            synapseConfiguration = _syml.GetOrSetDefault("Synapse", synapseConfiguration);
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
            synapseConfiguration = _syml.GetOrSetDefault("Synapse", new SynapseConfiguration());
        }
    }
}
