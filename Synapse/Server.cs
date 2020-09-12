using Synapse.Api;
using Synapse.Api.Events;
using Synapse.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Logger = Synapse.Api.Logger;

namespace Synapse
{
    public class Server
    {
        //Synapse Api
        public Logger Logger { get; } = new Logger();

        public Map Map { get; } = new Map();

        public FileLocations Files { get; } = new FileLocations();

        public EventHandler Events { get; } = new EventHandler();

        public ConfigHandler Configs { get; } = new ConfigHandler();

        public Player Host => PlayerManager.localPlayer.GetComponent<Player>();

        public List<Player> Players => PlayerManager.players.ToList().Select(x => x.GetComponent<Player>()).ToList();

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

        public List<TObject> GetObjectsOf<TObject>() where TObject : UnityEngine.Object => UnityEngine.Object.FindObjectsOfType<TObject>().ToList();

        public TObject GetObjectOf<TObject>() where TObject : UnityEngine.Object => UnityEngine.Object.FindObjectOfType<TObject>();


        //Vanilla Objects
        public ServerConsole ServerConsole => ServerConsole.singleton;

        public GameCore.Console GameConsole => GameCore.Console.singleton;


        public class FileLocations
        {
            //synapse
            private string synapseDiretory;

            //plugin
            private string mainPluginDirectory;
            private string pluginDirectory;
            private string sharedpluginDirectory;

            //config
            private string mainConfigDirectory;
            private string configDirectory;
            private string sharedConfigDirectory;

            //Synapse
            public string SynapseDirectory
            {
                get
                {
                    if (!Directory.Exists(synapseDiretory))
                        Directory.CreateDirectory(synapseDiretory);

                    return synapseDiretory;
                }
                private set => synapseDiretory = value;
            }

            //Plugin
            public string MainPluginDirectory
            {
                get
                {
                    if (!Directory.Exists(mainPluginDirectory))
                        Directory.CreateDirectory(mainPluginDirectory);

                    return mainPluginDirectory;
                }
                private set => mainPluginDirectory = value;
            }
            public string PluginDirectory
            {
                get
                {
                    if (!Directory.Exists(pluginDirectory))
                        Directory.CreateDirectory(pluginDirectory);

                    return pluginDirectory;
                }
                private set => pluginDirectory = value;
            }
            public string SharedPluginDirectory
            {
                get
                {
                    if (!Directory.Exists(sharedpluginDirectory))
                        Directory.CreateDirectory(sharedpluginDirectory);

                    return sharedpluginDirectory;
                }
                private set => sharedpluginDirectory = value;
            }

            //Config
            public string MainConfigDirectory
            {
                get
                {
                    if (!Directory.Exists(mainConfigDirectory))
                        Directory.CreateDirectory(mainConfigDirectory);

                    return mainConfigDirectory;
                }
                private set => mainConfigDirectory = value;
            }
            public string ConfigDirectory
            {
                get
                {
                    if (!Directory.Exists(configDirectory))
                        Directory.CreateDirectory(configDirectory);

                    return configDirectory;
                }
                private set => configDirectory = value;
            }
            public string SharedConfigDirectory
            {
                get
                {
                    if (!Directory.Exists(sharedConfigDirectory))
                        Directory.CreateDirectory(sharedConfigDirectory);

                    return sharedConfigDirectory;
                }
                private set => sharedConfigDirectory = value;
            }


            internal FileLocations() => Refresh();
            public void Refresh()
            {
                SynapseDirectory = Assembly.GetCallingAssembly().Location.Replace("Synapse.dll", "");

                MainPluginDirectory = Path.Combine(SynapseDirectory, "plugins");
                PluginDirectory = Path.Combine(MainPluginDirectory, $"server-{SynapseController.Server.Port}");
                SharedPluginDirectory = Path.Combine(MainPluginDirectory, "server-shared");

                MainConfigDirectory = Path.Combine(SynapseDirectory, "configs");
                ConfigDirectory = Path.Combine(MainConfigDirectory, $"server-{SynapseController.Server.Port}");
                SharedConfigDirectory = Path.Combine(MainConfigDirectory, "server-shared");
            }
        }
    }
}
