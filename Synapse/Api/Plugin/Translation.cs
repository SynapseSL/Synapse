using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Synapse.Api.Plugin
{
    public class Translation
    {
        internal Translation(PluginInformation info) => Information = info;

        private Dictionary<string, string> _rawtranslation;
        private Dictionary<string, string> _translation = new Dictionary<string, string>();
        internal PluginInformation Information { private get; set; }

        public void CreateTranslations(Dictionary<string, string> translations)
        {
            _rawtranslation = translations;
            var Path = SynapseController.Server.Files.GetOldTranslationFile(Information);

            if (!File.Exists(Path))
                File.Create(Path).Close();

            var dictionary = new Dictionary<string, string>();
            var lines = File.ReadAllLines(Path);
            var newlines = new List<string>();
            var position = 0;

            foreach (var pair in translations.ToList().Select(rawPair => new KeyValuePair<string, string>(rawPair.Key, rawPair.Value.Replace("\n", "\\n"))))
            {
                if (lines.Length > position)
                {
                    if (string.IsNullOrEmpty(lines[position]))
                    {
                        dictionary.Add(pair.Key, pair.Value);
                        newlines.Add(pair.Value);
                    }
                    else
                    {
                        dictionary.Add(pair.Key, lines[position]);
                        newlines.Add(lines[position]);
                    }
                }
                else
                {
                    dictionary.Add(pair.Key, pair.Value);
                    newlines.Add(pair.Value);
                }

                position++;
                File.WriteAllLines(Path, newlines.ToArray());
            }

            _translation = dictionary;
        }

        public void ReloadTranslations()
        {
            if (_rawtranslation != null)
                CreateTranslations(_rawtranslation);
        }

        public string GetTranslation(string translationName)
        {
            try
            {
                var trans = _translation.FirstOrDefault(x => x.Key == translationName).Value;
                return trans == null ? "Plugin requested a non-existing Translation!" : trans.Replace("\\n", "\n");
            }
            catch
            {
                return "Plugin requested a non-existing Translation!";
            }
        }
    }
}
