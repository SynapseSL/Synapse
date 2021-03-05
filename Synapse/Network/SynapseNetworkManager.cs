using System;
using System.Collections.Generic;
using System.Threading;
using Swan.Formatters;
using Synapse.Network.nodes;

namespace Synapse.Network
{
    public class SynapseNetworkManager
    {

        public SynapseNetworkClient Client;
        public SynapseNetworkServer Server;
        private Thread _reconnectLoop;

        public readonly List<INetworkNode> NetworkNodes = new List<INetworkNode>();

        public SynapseNetworkManager()
        {
            NetworkNodes.Add(new SynapseNetworkNode());
        }

        public void Start()
        {
            var synapseConfig = Synapse.Server.Get.Configs.synapseConfiguration;
            if (!synapseConfig.NetworkEnable) return;
            Client = new SynapseNetworkClient
            {
                ClientName = synapseConfig.NetworkName,
                Secret = synapseConfig.NetworkSecret,
                Url = synapseConfig.NetworkUrl
            };
            Client.Init();
            Synapse.Server.Get.Logger.Info("Synapse-Network Starting ClientLoop");
            StartClientReconnectLoop();
        }

        public void Shutdown()
        {
            Synapse.Server.Get.Logger.Info("Synapse-Network Shutdown");
            _reconnectLoop.Abort();
            Client.Disconnect();
            Server.Stop();
        }

        private void StartClientReconnectLoop()
        {
            _reconnectLoop = new Thread(async () =>
            {
                while (true)
                {
                    // Wait for 5.0 seconds
                    var availability = await Client.CheckAvailability();
                    if (!availability)
                    {

                        Synapse.Server.Get.Logger.Warn("Master-Ping failed");
                        
                        if (Synapse.Server.Get.Configs.synapseConfiguration.MasterAuthority)
                        {
                            BoostrapServer();
                            Thread.Sleep(250);
                            continue;
                        }
                        
                        if (Client.IsStarted)
                        {
                            Client.Disconnect();
                            Synapse.Server.Get.Logger.Warn("Synapse-Network client can't connect to Synapse-Network. Retrying in 5 seconds");
                            //Thread.Sleep(500 * Client.MigrationPriority);
                        }
                    }
                    else if (!Client.IsStarted)
                    {
                        Client.Connect();
                    }

                    Thread.Sleep(5000);
                }

                // ReSharper disable once FunctionNeverReturns
            });
            _reconnectLoop.Name = "Synapse-Network ReconnectLoop";
            _reconnectLoop.IsBackground = true;
            _reconnectLoop.Start();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public void BoostrapServer()
        {
            var synapseConfig = Synapse.Server.Get.Configs.synapseConfiguration;
            if (!synapseConfig.NetworkEnable || !synapseConfig.MasterAuthority) return;
            Synapse.Server.Get.Logger.Info($"Starting Synapse-Network Server");
            Server = SynapseNetworkServer.Instance;
            Server.Secret = synapseConfig.NetworkSecret;
            Server.Port = synapseConfig.NetworkPort;
            Server.Url = synapseConfig.NetworkUrl;
            Server.Stop();
            if (!SynapseNetworkServer.CheckServerPortAvailable(synapseConfig.NetworkPort))
            {
                Synapse.Server.Get.Logger.Error($"Port-Availability check for {synapseConfig.NetworkPort} failed: Port already in use");
                return;
            }
            Server.Start();
            Synapse.Server.Get.Logger.Info($"Synapse-Network Server running on port {synapseConfig.NetworkPort}");
        }
    }

    [Serializable]
    public class NetworkSyncEntry : SuccessfulStatus
    {
        public string Key { get; set; }
        public string Class { get; set; }
        public string Data { get; set; }

        [JsonProperty("_", true)]
        public Type ParseTypeData => Type.GetType(Class);

        [JsonProperty("__", true)]
        public bool IsCoreType => ParseTypeData?.AssemblyQualifiedName?.Contains("mscorlib") ?? true;
        
        public static bool CheckIsCoreType(Type type) => type?.AssemblyQualifiedName?.Contains("mscorlib") ?? true;
        
        private object _valStore;
        
        public T Value<T>()
        {
            if (_valStore != null) return (T) _valStore;
            var val = Parse();
            _valStore = val;
            return (T) val;
        }
        
        public object Value()
        {
            if (_valStore != null) return _valStore;
            var val = Parse();
            _valStore = val;
            return val;
        }

        public void Update<T>(T obj)
        {
            _valStore = null;
            Data = Serialize(obj);
            Class = obj.GetType().AssemblyQualifiedName;
        }
        
        public static NetworkSyncEntry FromPair<T>(string key, T value)
        {
            var type = value.GetType();
            return new NetworkSyncEntry
            {
                Key = key,
                Data = Serialize(value),
                Class = type.AssemblyQualifiedName.Contains("mscorlib") ? type.Name : type.AssemblyQualifiedName
            };
        }

        public object Parse()
        {
            var t = ParseTypeData;
            if (IsCoreType)
            {         
#if DEBUG
                Server.Get.Logger.Info("Primitive DataType Deserialization");
#endif
                if (t == typeof(string)) return Data;
                if (t == typeof(int)) return int.Parse(Data);
                if (t == typeof(float)) return float.Parse(Data);
                if (t == typeof(bool)) return bool.Parse(Data);
                if (t == typeof(long)) return long.Parse(Data);
                if (t == typeof(double)) return double.Parse(Data);
                if (t == typeof(short)) return short.Parse(Data);
                if (t == typeof(byte)) return byte.Parse(Data);
            }
            
            return Json.Deserialize(Data, t);
        }
        
        public static string Serialize(object obj)
        {
            var t = obj.GetType();
            if (CheckIsCoreType(t))
            {
#if DEBUG
                Server.Get.Logger.Info("Primitive DataType Serialization");
#endif
                return obj.ToString();
            }
            return Json.Serialize(obj);
        }

        protected bool Equals(NetworkSyncEntry other)
        {
            return Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NetworkSyncEntry) obj);
        }

        public override int GetHashCode()
        {
            return (Key != null ? Key.GetHashCode() : 0);
        }
    }
}