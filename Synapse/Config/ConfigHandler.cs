using Synapse.Translation;

namespace Synapse.Config
{
    public class ConfigHandler
    {
        internal ConfigHandler() { }

        internal SynapseConfiguration SynapseConfiguration { get; set; }
        internal SynapseTranslation<Translation.Translation> SynapseTranslation { get; set; }

        private SYML _syml;
        
        internal void Init()
        {
            SynapseTranslation = new SynapseTranslation<Translation.Translation>(Server.Get.Files.GetTranslationPath("Synapse"));
            SynapseTranslation.AddTranslation(new Synapse.Translation.Translation(), "ENGLISH");
            SynapseTranslation.AddTranslation(new Translation.Translation
            {
                noPermissions = "Du hast keine Berechtigung diesen Command zu benutzen!(%perm%)",
                sameTeam = "<b>Du kannst diese Person nicht verletzen!</b>",
                scpTeam = "Als deine jetzige Rolle kannst du nichts machen was ein SCP verletzen würde!",
            }, "GERMAN");
            SynapseTranslation.AddTranslation(new Translation.Translation
            {
                noPermissions = "Vous ne disposez pas de la permission requise pour cette commande ! (%perm%)",
                scpTeam = "Vous ne pouvez pas nuire à un SCP en cette classe !",
                sameTeam = "Vous ne pouvez pas nuire à cette personne !",
            }, "FRENCH");

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
            SynapseConfiguration = _syml.GetOrSetDefault("Synapse", new SynapseConfiguration());
        }
    }
}
