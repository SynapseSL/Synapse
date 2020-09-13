using System.IO;

namespace Synapse.Api.Plugin
{
    public interface IPlugin
    {
        PluginInformations Informations { get; set; }

        Translation Translation { get; set; }

        string PluginDirectory { get; set; }

        void Load();

        void ReloadConfigs();
    }


    public abstract class AbstractPlugin : IPlugin
    {
        private string pluginDirectory;

        public virtual void Load()
        {  
        }

        public virtual void ReloadConfigs()
        {
        }

        public Translation Translation { get; set; }

        public PluginInformations Informations { get; set; }

        public string PluginDirectory
        {
            get
            {
                if (pluginDirectory == null)
                    return null;

                if (!Directory.Exists(pluginDirectory))
                    Directory.CreateDirectory(pluginDirectory);

                return pluginDirectory;
            }
            set => pluginDirectory = value;
        } 
    }
}