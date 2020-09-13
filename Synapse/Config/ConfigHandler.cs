﻿using System.Linq;
using Harmony;
using Synapse.Api;

namespace Synapse.Config
{
    public class ConfigHandler
    {
        internal ConfigHandler() { }

        private SYML _syml;
        
        public void Init()
        {
            _syml = new SYML(SynapseController.Server.Files.ConfigFile);
            _syml.Load();
            SynapseConfiguration configuration = new SynapseConfiguration();
            configuration = _syml.GetOrSetDefault("Synapse", configuration);
            SynapseController.Server.Logger.Warn(configuration.ToString());
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
            //TODO: Permission Reload + Automatic Plugin Config Reload
        }
        
    }

    public class SynapseConfiguration : IConfigSection
    {
        public string serverName { get; set; } = "A new awesome server";
        public string joinBroadcast { get; set; } = "Welcome % player";

        public string[] keycards { get; set; } = {"Scientist","MajorScientist"};

        public SerializedMapPoint[] mapPoints = { new SerializedMapPoint("Test",0,10,0)};
        
        public override string ToString()
        {
            return $"SynapseConfiguration(serverName={serverName} joinBroadcast={joinBroadcast} keycards={keycards.Join(delimiter: ", ")} " +
                   $"points={mapPoints.ToList().Select(f => f.ToString()).Join(delimiter:", ")})";
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
