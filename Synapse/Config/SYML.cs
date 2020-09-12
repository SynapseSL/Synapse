using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Synapse.Config
{
    public class SYML
    {
        private string path;

        public Dictionary<string, ConfigSection> Sections = new Dictionary<string, ConfigSection>();
        
        public SYML(string path)
        {
            this.path = path;
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
        }

        public void Load()
        {
            var text = File.ReadAllText(path);
            Sections = ParseString(text);
        }

        public T GetOrSetDefault<T>(string section, T defValue) where T : IConfigSection
        {
            if (Sections.ContainsKey(section))
            {
                return Sections[section].LoadAs<T>();
            }
            else
            {
                ConfigSection cfgs = new ConfigSection(section, "");
                cfgs.Import(defValue);
                Sections[section] = cfgs;
                Store();
                return defValue;
            }
        }

        public object GetOrSetDefaultUnsafe(string section, object obj) 
        {
            if (Sections.ContainsKey(section))
            {
                return Sections[section].LoadAsType(obj.GetType());
            }
            else
            {
                ConfigSection cfgs = new ConfigSection(section, "");
                cfgs.ImportUnsafe(obj);
                Sections[section] = cfgs;
                Store();
                return obj;
            }
        }

        
        public void Store()
        {
            var text = WriteSections(Sections);
            File.WriteAllText(path, text);
        }
        
        private static Dictionary<string,ConfigSection> ParseString(string str)
        {
            var sections = new Dictionary<string, ConfigSection>();
            var split =  str.Split(new string[] {"::"}, StringSplitOptions.None);

            for (var i = 1; i < split.Length; i+=2)
            {
                var identifier = split[i];
                var content = split[i + 1];
                
                int lastBracket = content.Length - 1;
                int firstBracket = 0;
                for (var i1 = 0; i1 < content.Length; i1++)
                {
                    if (content[i1] == '{')
                    {
                        firstBracket = i1;
                        break;
                    } 
                }
                for (var i1 = 0; i1 < content.Length; i1++)
                {
                    if (content[i1] == '}')
                    {
                        lastBracket = i1;
                    } 
                }
                content = content.Substring(firstBracket + 1, lastBracket - firstBracket - 1);
                sections.Add(identifier, new ConfigSection(identifier,content));
            }
            
            return sections;
        }

        [CanBeNull]
        private static string WriteSections(Dictionary<string, ConfigSection> sections)
        {
            string s = "";
            foreach (var value in sections.Values)
            {
                s += value.Serialize();
                s += "\n";
            }

            return s;
        }
    }

    public interface IConfigSection { }

    public class ConfigSection
    {
        public ConfigSection() { }

        public ConfigSection(string section, string content)
        {
            Section = section;
            Content = content;
        }

        public string Section { get; set; }
        public string Content { get; set; }
        
        public T LoadAs<T>() where T: IConfigSection
        {
            try
            {
                SynapseController.Server.Logger.Info($"Deserializing section {Section}");
                var ret = new DeserializerBuilder().Build().Deserialize<T>(Content);
                SynapseController.Server.Logger.Info("Deserialization done");
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public object LoadAsType(Type type)
        {
            try
            {
                SynapseController.Server.Logger.Info($"Deserializing section {Section} unsafely with type {type.Name}");
                var ret = new DeserializerBuilder().Build().Deserialize(Content, type);
                SynapseController.Server.Logger.Info("Deserialization done");
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        
        public string Import<T>(T t) where T: IConfigSection
        {
            Content = new SerializerBuilder().Build().Serialize((object)t);

            return Content;
        }

        public string ImportUnsafe(object obj)
        {
            Content = new SerializerBuilder().Build().Serialize(obj);
            return Content;
        }

        
        public string Serialize()
        {
            return "::" + Section + "::" + "\n" + "{\n" + Content.Trim() + "\n}\n";
        }
    }
}