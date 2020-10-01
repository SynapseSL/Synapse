using System.Collections.Generic;
using System.Globalization;
using Synapse.Api;
using Synapse.Api.Plugin;

namespace Synapse.Config
{
    public class ConfigHandler
    {
        internal ConfigHandler() { }

        internal SynapseConfiguration SynapseConfiguration;
        internal Translation SynapseTranslation;

        private SYML _syml;
        
        public void Init()
        {
            SynapseTranslation = new Translation(new PluginInformations { Name = "Synapse" });
            var trans = new Dictionary<string, string>
            {
                {"sameteam","You cant harm this Person" },
                {"scpteam","As your current Role cant you harm an Scp"  },
                {"groupgranted","Your have entered the Password for group: %group%" },
                {"wrongpw", "You have entered a incorrect Password"}
            };
            SynapseTranslation.CreateTranslations(trans);

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
            SynapseTranslation.ReloadTranslations();
        }
        
    }

    public class SerializedMapPoint
    {
        public SerializedMapPoint(string room, float x, float y, float z)
        {
            this.Room = room;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        
        public SerializedMapPoint(MapPoint point)
        {
            Room = point.Room.RoomName;
            X = point.RelativePosition.x;
            Y = point.RelativePosition.y;
            Z = point.RelativePosition.z;
        }
        
        public SerializedMapPoint()
        {
        }

        public string Room { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public MapPoint parse()
        {
            return MapPoint.Parse(ToString());
        }
        
        public override string ToString()
        {
            return $"{Room}:{X.ToString(CultureInfo.InvariantCulture)}:{Y.ToString(CultureInfo.InvariantCulture)}:{Z.ToString(CultureInfo.InvariantCulture)}";
        }
    }
    
}
