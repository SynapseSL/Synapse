using Synapse.Config;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Translation
{
    public class SynapseTranslation<TPluginTranslation> where TPluginTranslation : IPluginTranslation
    {
        public SynapseTranslation(string path)
        {
            translationSYML = new SYML(path);
            translationSYML.Load();
        }

        private readonly SYML translationSYML;

        public TPluginTranslation ActiveTranslation
        {
            get
            {
                return translationSYML.Sections.Keys.Any(x => x == Server.Get.Configs.SynapseConfiguration.Language.ToUpper())
                    ? translationSYML.Sections.First(x => x.Key.ToUpper() == Server.Get.Configs.SynapseConfiguration.Language.ToUpper()).Value.LoadAs<TPluginTranslation>()
                    : translationSYML.Sections.Keys.Any(x => x == "ENGLISH")
                    ? translationSYML.Sections.First(x => x.Key.ToUpper() == "ENGLISH").Value.LoadAs<TPluginTranslation>()
                    : translationSYML.Sections.FirstOrDefault().Value.LoadAs<TPluginTranslation>();
            }
        }

        public TPluginTranslation this[string translation] => translationSYML.Sections.FirstOrDefault(x => x.Key.ToUpper() == translation.ToUpper()).Value.LoadAs<TPluginTranslation>();

        public TPluginTranslation AddTranslation(TPluginTranslation translation, string language = "ENGLISH") => translationSYML.GetOrSetDefault(language, translation);

        public List<string> Languages => translationSYML.Sections.Keys.ToList();

        public void Reload() => translationSYML.Load();
    }

    public interface IPluginTranslation : IConfigSection { }
}
