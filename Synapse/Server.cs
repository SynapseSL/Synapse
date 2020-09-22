using Synapse.Api;
using Synapse.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Logger = Synapse.Api.Logger;
using EventHandler = Synapse.Api.Events.EventHandler;
using Synapse.Api.Plugin;
using GameCore;
using Synapse.Api.Roles;

namespace Synapse
{
    public class Server
    {
        internal Server() { }

        public static Server Get => SynapseController.Server;

        //Synapse Api
        public Logger Logger { get; } = new Logger();

        public Map Map { get; } = new Map();

        public RoleManager RoleManager { get; } = new RoleManager();

        public FileLocations Files { get; } = new FileLocations();

        public EventHandler Events { get; } = new EventHandler();

        public ConfigHandler Configs { get; } = new ConfigHandler();

        public Player Host
        {
            get
            {
                if (PlayerManager.localPlayer.GetComponent<Player>() == null)
                    PlayerManager.localPlayer.AddComponent<Player>();

                return PlayerManager.localPlayer.GetComponent<Player>();
            }
        }

        //Server fields
        public ushort Port
        {
            get => ServerStatic.ServerPort;
            set => ServerStatic.ServerPort = value;
        }

        public string Name
        {
            get => ServerConsole._serverName;
            set
            {
                ServerConsole._serverName = value;
                ServerConsole.RefreshServerName();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public bool FF
        {
            get => ServerConsole.FriendlyFire;
            set => ServerConsole.FriendlyFire = value;
        }



        public List<TObject> GetObjectsOf<TObject>() where TObject : UnityEngine.Object => UnityEngine.Object.FindObjectsOfType<TObject>().ToList();

        public TObject GetObjectOf<TObject>() where TObject : UnityEngine.Object => UnityEngine.Object.FindObjectOfType<TObject>();

        public List<Player> Players => PlayerManager.players.ToList().Select(x => x.GetComponent<Player>()).ToList();

        public List<Player> GetPlayers(Func<Player,bool> func) => Players.Where(func).ToList();

        public Player GetPlayer(string argument)
        {
            var players = Players;

            if (int.TryParse(argument, out var playerid))
            {
                var player = GetPlayer(playerid);
                if (player == null)
                    goto AA_001;

                return player;
            }

            else if (argument.Contains("@"))
            {
                var player = players.FirstOrDefault(x => x.UserId.ToLower() == argument);
                if (player == null)
                    goto AA_001;

                return player;
            }

        AA_001:
            return players.FirstOrDefault(x => x.NickName.ToLower() == argument.ToLower());
        }

        public Player GetPlayer(int playerid) => Players.FirstOrDefault(x => x.PlayerId == playerid);


        //Vanilla Objects
        public ServerConsole ServerConsole => ServerConsole.singleton;

        public GameCore.Console GameConsole => GameCore.Console.singleton;


        public class FileLocations
        {
            //synapse
            private string _synapseDirectory;
            
            //database
            private string _databaseDirectory;

            //plugin
            private string _mainPluginDirectory;
            private string _pluginDirectory;
            private string _sharedPluginDirectory;

            //config
            private string _mainConfigDirectory;
            private string _configDirectory;
            private string _sharedConfigDirectory;

            private string _configFile;
            //Synapse
            public string SynapseDirectory
            {
                get
                {
                    if (!Directory.Exists(_synapseDirectory))
                        Directory.CreateDirectory(_synapseDirectory);

                    return _synapseDirectory;
                }
                private set => _synapseDirectory = value;
            }
            
            public string DatabaseDirectory
            {
                get
                {
                    if (!Directory.Exists(_databaseDirectory))
                        Directory.CreateDirectory(_databaseDirectory);

                    return _databaseDirectory;
                }
                private set => _databaseDirectory = value;
            }
            public string DatabaseFile { get => Path.Combine(DatabaseDirectory, "database.db"); }
            
            //Plugin
            public string MainPluginDirectory
            {
                get
                {
                    if (!Directory.Exists(_mainPluginDirectory))
                        Directory.CreateDirectory(_mainPluginDirectory);

                    return _mainPluginDirectory;
                }
                private set => _mainPluginDirectory = value;
            }
            public string PluginDirectory
            {
                get
                {
                    if (!Directory.Exists(_pluginDirectory))
                        Directory.CreateDirectory(_pluginDirectory);

                    return _pluginDirectory;
                }
                private set => _pluginDirectory = value;
            }
            public string SharedPluginDirectory
            {
                get
                {
                    if (!Directory.Exists(_sharedPluginDirectory))
                        Directory.CreateDirectory(_sharedPluginDirectory);

                    return _sharedPluginDirectory;
                }
                private set => _sharedPluginDirectory = value;
            }

            //Config
            public string MainConfigDirectory
            {
                get
                {
                    if (!Directory.Exists(_mainConfigDirectory))
                        Directory.CreateDirectory(_mainConfigDirectory);

                    return _mainConfigDirectory;
                }
                private set => _mainConfigDirectory = value;
            }
            public string ConfigDirectory
            {
                get
                {
                    if (!Directory.Exists(_configDirectory))
                        Directory.CreateDirectory(_configDirectory);

                    return _configDirectory;
                }
                private set => _configDirectory = value;
            }
            public string SharedConfigDirectory
            {
                get
                {
                    if (!Directory.Exists(_sharedConfigDirectory))
                        Directory.CreateDirectory(_sharedConfigDirectory);

                    return _sharedConfigDirectory;
                }
                private set => _sharedConfigDirectory = value;
            }

            public string ConfigFile
            {
                get
                {
                    if (!File.Exists(_configFile))
                        File.Create(_configFile).Close();

                    return _configFile;
                }
                internal set => _configFile = value;
            }


            internal FileLocations() => Refresh();
            public void Refresh()
            {
                SynapseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse");
                DatabaseDirectory = Path.Combine(SynapseDirectory, "database");
                
                MainPluginDirectory = Path.Combine(SynapseDirectory, "plugins");
                PluginDirectory = Path.Combine(MainPluginDirectory, $"server-{ServerStatic.ServerPort}");
                SharedPluginDirectory = Path.Combine(MainPluginDirectory, "server-shared");

                MainConfigDirectory = Path.Combine(SynapseDirectory, "configs");
                ConfigDirectory = Path.Combine(MainConfigDirectory, $"server-{ServerStatic.ServerPort}");
                SharedConfigDirectory = Path.Combine(MainConfigDirectory, "server-shared");

                ConfigFile = Path.Combine(ConfigDirectory, "config.syml");
            }
            public string GetTranslationFile(PluginInformations infos)
            {
                if (File.Exists(Path.Combine(SharedConfigDirectory, infos.Name + "-translation.txt")))
                    return Path.Combine(SharedConfigDirectory, infos.Name + "-translation.txt");

                return Path.Combine(ConfigDirectory, infos.Name + "-translation.txt");
            }

            public string GetPluginDirectory(PluginInformations infos)
            {
                
                if (infos.shared)
                    return Path.Combine(SharedPluginDirectory, infos.Name);
                return Path.Combine(PluginDirectory, infos.Name);
            }
        }
    }
}
