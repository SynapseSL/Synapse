using System.Linq;
using System.Collections.Generic;
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

        private readonly SYML translationSYML;

        public TPluginTranslation ActiveTranslation
        {
            get
            {
                if (translationSYML.Sections.TryFind(out var _v1, x => x.Key.ToUpper() == Server.Get.Configs.SynapseConfiguration.Language.ToUpper()))
                    return _v1.Value.LoadAs<TPluginTranslation>();

                if (translationSYML.Sections.TryFind(out var _v2, x => x.Key.ToUpper() == "ENGLISH"))
                    return _v2.Value.LoadAs<TPluginTranslation>();

                return translationSYML.Sections.FirstOrDefault().Value.LoadAs<TPluginTranslation>();
            }
        }

        public TPluginTranslation this[string translation] => translationSYML.Sections.FirstOrDefault(x => x.Key.ToUpper() == translation.ToUpper()).Value.LoadAs<TPluginTranslation>();

        public TPluginTranslation AddTranslation(TPluginTranslation translation, string language = "ENGLISH") => translationSYML.GetOrSetDefault(language, translation);

        public List<string> Languages => translationSYML.Sections.Keys.ToList();

        public void Reload() => translationSYML.Load();
    }

    public interface IPluginTranslation : IConfigSection { }
}