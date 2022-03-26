using Synapse.Api;
using Synapse.Api.CustomObjects;
using Synapse.Api.Enum;
using Synapse.Api.Items;
using Synapse.Api.Plugin;
using Synapse.Api.Roles;
using Synapse.Api.Teams;
using Synapse.Config;
using Synapse.Permission;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Console = GameCore.Console;
using EventHandler = Synapse.Api.Events.EventHandler;
using Logger = Synapse.Api.Logger;
using Object = UnityEngine.Object;

namespace Synapse
{
    public class Server
    {
        internal Server()
        {
        }

        public static Server Get => SynapseController.Server;

        //Synapse Api
        public Logger Logger { get; } = new Logger();

        public Map Map { get; } = new Map();

        public FileLocations Files { get; } = new FileLocations();

        public EventHandler Events { get; } = new EventHandler();

        public RoleManager RoleManager { get; } = new RoleManager();

        public TeamManager TeamManager { get; } = new TeamManager();

        public ItemManager ItemManager { get; } = new ItemManager();

        public ConfigHandler Configs { get; } = new ConfigHandler();

        public PermissionHandler PermissionHandler { get; } = new PermissionHandler();

        public SchematicHandler Schematic { get; } = new SchematicHandler();

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

        public int PlayersAmount => ServerConsole.PlayersAmount;

        public int Slots
        {
            get => CustomNetworkManager.slots;
            set => CustomNetworkManager.slots = value;
        }

        public bool FF
        {
            get => ServerConsole.FriendlyFire;
            set {
                ServerConsole.FriendlyFire = value;
                ServerConfigSynchronizer.RefreshAllConfigs();
            }
        }

        public string[] Colors => AllowedColors.Values.ToArray();

        public Dictionary<Misc.PlayerInfoColorTypes, string> AllowedColors => Misc.AllowedColors;

        public List<Player> Players =>
            PlayerManager.players.Select(x => x.GetComponent<Player>()).Where(x => !x.IsDummy).ToList();

        private List<Player> PlayerObjects =>
            PlayerManager.players.Select(x => x.GetComponent<Player>()).ToList();

        //Vanilla Objects
        public ServerConsole ServerConsole => ServerConsole.singleton;

        public Console GameConsole => Console.singleton;


        public void Reload()
        {
            try
            {
                Configs.Reload();
                PermissionHandler.Reload();
                Schematic.Load();
                SynapseController.PluginLoader.ReloadConfigs();
            }
            catch(Exception e)
            {
                Logger.Error($"Error ocurred while reloading Synapse:\n{e}");
            }
        }

        /// <summary>
        ///     Bans a player that is not on the server
        /// </summary>
        /// <param name="reason">The reason for the ban</param>
        /// <param name="issuer">The person/SCP that banned the player</param>
        /// <param name="id">The person account id (e.g. xxxxxxxxxxx@steam) to ban</param>
        /// <param name="duration">The duration for the ban  in seconds</param>
        public void OfflineBanID(string reason, string issuer, string id, int duration)
        {
            BanHandler.IssueBan(new BanDetails
            {
                Reason = reason,
                Issuer = issuer,
                Id = id,
                OriginalName = "Unknown - offline ban",
                IssuanceTime = DateTime.UtcNow.Ticks,
                Expires = DateTime.UtcNow.AddSeconds(duration).Ticks
            }, BanHandler.BanType.UserId);
        }

        /// <summary>
        ///     Bans a IP
        /// </summary>
        /// <param name="reason">The reason for the ban</param>
        /// <param name="issuer">The person/SCP that banned the player</param>
        /// <param name="ip">The IPv4 or IPv6 to ban</param>
        /// <param name="duration">The duration for the ban in seconds</param>
        public void OfflineBanIP(string reason, string issuer, string ip, int duration)
        {
            BanHandler.IssueBan(new BanDetails
            {
                Reason = reason,
                Issuer = issuer,
                Id = ip,
                OriginalName = "Unknown - offline ban",
                IssuanceTime = DateTime.UtcNow.Ticks,
                Expires = DateTime.UtcNow.AddSeconds(duration).Ticks
            }, BanHandler.BanType.IP);
        }

        public List<TObject> GetObjectsOf<TObject>() where TObject : Object
        {
            return Object.FindObjectsOfType<TObject>().ToList();
        }

        public TObject GetObjectOf<TObject>() where TObject : Object
        {
            return Object.FindObjectOfType<TObject>();
        }

        public List<Player> GetPlayers(Func<Player, bool> func)
        {
            return Players.Where(func).ToList();
        }

        public bool TryGetPlayers(string arg, out List<Player> playerList, Player me = null)
        {
            var players = new List<Player>();
            var args = arg.Split('.');

            foreach (var parameter in args)
            {
                if (string.IsNullOrWhiteSpace(parameter)) continue;

                switch (parameter.ToUpper())
                {
                    case "SELF":
                    case "ME":
                        if (me == null) continue;

                        if (!players.Contains(me))
                            players.Add(me);
                        continue;

                    case "REMOTEADMIN":
                    case "ADMIN":
                    case "STAFF":
                        foreach (var player in Players)
                            if (player.ServerRoles.RemoteAdmin)
                                if (!players.Contains(player))
                                    players.Add(player);
                        continue;

                    case "NW":
                    case "NORTHWOOD":
                        foreach (var player in Players)
                            if (player.ServerRoles.Staff)
                                if (!players.Contains(player))
                                    players.Add(player);
                        break;

                    case "*":
                    case "ALL":
                    case "EVERYONE":
                        foreach (var player2 in Server.Get.Players)
                            if (!players.Contains(player2))
                                players.Add(player2);
                        continue;

                    default:
                        var player3 = GetPlayer(parameter);
                        if (player3 == null) continue;
                        if (!players.Contains(player3))
                            players.Add(player3);
                        continue;
                }
            }

            playerList = players;

            return players.Count != 0;
        }

        public Player GetPlayer(string argument)
        {
            var players = Players;

            if (argument.Contains("@"))
            {
                var player = GetPlayerByUID(argument);
                if (player != null)
                    return player;
            }

            if (int.TryParse(argument, out var playerid))
            {
                var player = GetPlayer(playerid);
                if (player != null)
                    return player;
            }

            return players.FirstOrDefault(x => x.NickName.ToLower() == argument.ToLower());
        }

        public Player GetPlayer(int playerid)
        {
            return PlayerObjects.FirstOrDefault(x => x.PlayerId == playerid);
        }

        public Player GetPlayer(uint netID) => PlayerObjects.FirstOrDefault(x => x.NetworkIdentity.netId == netID);

        public Player GetPlayerByUID(string uid)
        {
            return Players.FirstOrDefault(x => x.UserId == uid || x.SecondUserID == uid);
        }


        public class FileLocations
        {
            private string _configDirectory;

            private string _configFile;

            //database
            private string _databaseDirectory;

            //config
            private string _mainConfigDirectory;
            private string _sharedConfigDirectory;
            private string _permissionFile;

            //plugin
            private string _mainPluginDirectory;
            private string _pluginDirectory;
            private string _sharedPluginDirectory;

            //Schematic
            private string _schematicDirectory;

            //Logs
            private string _logDirectory;
            private string _logPortDirectory;

            //synapse
            private string _synapseDirectory;


            internal FileLocations() => Refresh();

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

            public string DatabaseFile => Path.Combine(DatabaseDirectory, "database.db");

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

            public string SchematicDirectory
            {
                get
                {
                    if(!Directory.Exists(_schematicDirectory))
                        Directory.CreateDirectory(_schematicDirectory);

                    return _schematicDirectory;
                }
                private set => _schematicDirectory = value;
            }

            public string LogDirectory
            {
                get
                {
                    if (!Directory.Exists(_logDirectory))
                        Directory.CreateDirectory(_logDirectory);

                    return _logDirectory;
                }
                private set => _logDirectory = value;
            }

            public string LogPortDirectory
            {
                get
                {
                    if (!Directory.Exists(_logPortDirectory))
                        Directory.CreateDirectory(_logPortDirectory);

                    return _logPortDirectory;
                }
                private set => _logPortDirectory = value;
            }

            public string LogFile
            {
                get
                {
                    var date = DateTime.Now;
                    var file = Path.Combine(LogPortDirectory, $"Synapse {date.Year}-{date.Month}-{date.Day}.txt");

                    if (!File.Exists(file))
                        File.Create(file).Close();

                    return file;
                }
            }

            public string PermissionFile
            {
                get
                {
                    if (!File.Exists(_permissionFile))
                        File.Create(_permissionFile).Close();

                    return _permissionFile;
                }
                set => _permissionFile = value;
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

            public void Refresh()
            {
                var localpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Synapse");
                SynapseDirectory = Directory.Exists(localpath)
                    ? localpath
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Synapse");
                DatabaseDirectory = Path.Combine(SynapseDirectory, "database");

                MainPluginDirectory = Path.Combine(SynapseDirectory, "plugins");
                PluginDirectory = Path.Combine(MainPluginDirectory, $"server-{ServerStatic.ServerPort}");
                SharedPluginDirectory = Path.Combine(MainPluginDirectory, "server-shared");

                MainConfigDirectory = Path.Combine(SynapseDirectory, "configs");
                ConfigDirectory = Path.Combine(MainConfigDirectory, $"server-{ServerStatic.ServerPort}");
                SharedConfigDirectory = Path.Combine(MainConfigDirectory, "server-shared");

                SchematicDirectory = Path.Combine(SynapseDirectory, "schematics");

                LogDirectory = Path.Combine(SynapseDirectory, "logs");
                LogPortDirectory = Path.Combine(LogDirectory, "server-" + ServerStatic.ServerPort);

                var configpath = Path.Combine(ConfigDirectory, "config.syml");
                ConfigFile = File.Exists(configpath) ? configpath : Path.Combine(SharedConfigDirectory, "config.syml");


                var permissionspath = Path.Combine(ConfigDirectory, "permission.syml");
                PermissionFile = File.Exists(permissionspath)
                    ? permissionspath
                    : Path.Combine(SharedConfigDirectory, "permission.syml");
            }

            public string GetOldTranslationFile(PluginInformation infos)
            {
                if (File.Exists(Path.Combine(SharedConfigDirectory, infos.Name + "-translation.txt")))
                    return Path.Combine(SharedConfigDirectory, infos.Name + "-translation.txt");

                return Path.Combine(ConfigDirectory, infos.Name + "-translation.txt");
            }

            public string GetTranslationPath(string name)
            {
                var translationpath = Path.Combine(ConfigDirectory, name + "-translation.syml");
                return File.Exists(translationpath)
                    ? translationpath
                    : Path.Combine(SharedConfigDirectory, name + "-translation.syml");
            }

            public string GetPluginDirectory(PluginInformation infos)
            {
                if (infos.shared)
                    return Path.Combine(SharedPluginDirectory, infos.Name);
                return Path.Combine(PluginDirectory, infos.Name);
            }
        }
    }
}
