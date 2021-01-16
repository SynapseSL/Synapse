using System.Linq;
using Synapse.Config;

namespace Synapse.Translation
{
    public class SynapseTranslation<TPluginTranslation> where TPluginTranslation : IPluginTranslation
    {
        public SynapseTranslation(string path)
        {
            translationSYML = new SYML(path);
            translationSYML.Load();
        }

        private SYML translationSYML;

        public TPluginTranslation ActiveTranslation
        {
            get
            {
                var serverTranslation = translationSYML.Sections.FirstOrDefault(x => x.Key.ToUpper() == Server.Get.Configs.synapseConfiguration.Language.ToUpper()).Value.LoadAs<TPluginTranslation>();
                if (serverTranslation != null) return serverTranslation;

                var englishTranslation = translationSYML.Sections.FirstOrDefault(x => x.Key.ToUpper() == "ENGLISH").Value.LoadAs<TPluginTranslation>();
                if (englishTranslation != null) return englishTranslation;

                return translationSYML.Sections.FirstOrDefault().Value.LoadAs<TPluginTranslation>();
            }
        }

        public TPluginTranslation this[string translation] => translationSYML.Sections.FirstOrDefault(x => x.Key.ToUpper() == translation.ToUpper()).Value.LoadAs<TPluginTranslation>();

        public TPluginTranslation AddTranslation(TPluginTranslation translation, string language = "ENGLISH") => translationSYML.GetOrSetDefault(language, translation);

        public void Reload() => translationSYML.Load();
    }

    public interface IPluginTranslation : IConfigSection { }
}
