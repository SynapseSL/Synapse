using System.IO;
using System;

namespace Synapse.Api.Plugin
{
    public interface IPlugin
    {
        PluginInformation Information { get; set; }

        [Obsolete("The old Translation System is not recommended to use",false)]
        Translation Translation { get; set; }

        string PluginDirectory { get; set; }

        void Load();

        void ReloadConfigs();
    }


    public abstract class AbstractPlugin : IPlugin
    {
        private string _pluginDirectory;

        public virtual void Load() => Logger.Get.Info($"{Information.Name} by {Information.Author} has loaded!");

        public virtual void ReloadConfigs()
        {
        }

        [Obsolete("The old Translation System is not recommended to use", false)]
        public Translation Translation { get; set; }

        public PluginInformation Information { get; set; }

        public string PluginDirectory
        {
            get
            {
                if (_pluginDirectory == null)
                    return null;

                if (!Directory.Exists(_pluginDirectory))
                    Directory.CreateDirectory(_pluginDirectory);

                return _pluginDirectory;
            }
            set => _pluginDirectory = value;
        } 
    }
}