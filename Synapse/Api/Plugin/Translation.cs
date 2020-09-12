using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Synapse.Api.Plugin
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Translation
    {
        internal Translation(string plname) => name = plname;

        private Dictionary<string, string> _rawtranslation;
        private Dictionary<string, string> _translation = new Dictionary<string, string>();
        internal string name { private get; set; }

        public void CreateTranslations(Dictionary<string, string> translations)
        {
            _rawtranslation = translations;

            var translationPath = SynapseController.Server.Files.GetTranslationFile(name);
            var dictionary = new Dictionary<string, string>();
            var lines = File.ReadAllLines(translationPath);
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
                File.WriteAllLines(translationPath, newlines.ToArray());
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
                return trans == null ? "Plugin requested a not created Translation!" : trans.Replace("\\n", "\n");
            }
            catch
            {
                return "Plugin requested a not created Translation!";
            }
        }
    }
}
