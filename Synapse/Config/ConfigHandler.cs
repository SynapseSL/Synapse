using System.Linq;
using Harmony;
using Synapse.Api;

namespace Synapse.Config
{
    public class ConfigHandler
    {
        internal ConfigHandler() { }

        internal SynapseConfiguration SynapseConfiguration;

        private SYML _syml;
        
        public void Init()
        {
            _syml = new SYML(SynapseController.Server.Files.ConfigFile);
            _syml.Load();
            SynapseConfiguration = new SynapseConfiguration();
            SynapseConfiguration = _syml.GetOrSetDefault("Synapse", SynapseConfiguration);
            SynapseController.Server.Logger.Warn(SynapseConfiguration.ToString());
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
            SynapseController.PluginLoader.ReloadConfigs();
            SynapseController.CommandHandlers.ReloadAll();
            //TODO: Permission Reload
        }
        
    }

    public class SerializedMapPoint
    {
        public SerializedMapPoint(string room, float x, float y, float z)
        {
            this.room = room;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public SerializedMapPoint(MapPoint point)
        {
            room = point.Room.RoomName;
            x = point.RelativePosition.x;
            y = point.RelativePosition.y;
            z = point.RelativePosition.z;
        }
        
        public SerializedMapPoint()
        {
        }

        public string room { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public MapPoint parse()
        {
            return MapPoint.Parse(ToString());
        }
        
        public override string ToString()
        {
            return $"{room}:{x.ToString()}:{y.ToString()}:{z.ToString()}";
        }
    }
    
}
