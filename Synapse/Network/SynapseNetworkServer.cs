using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using EmbedIO;
using EmbedIO.WebApi;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Swan;
using Synapse.Api;
using Synapse.Network.Models;
using Synapse.Network.Routes;
using Synapse.Reactive;

namespace Synapse.Network
{
    public class SynapseNetworkServer
    {
        private static SynapseNetworkServer _instance;

        public readonly HashSet<NetworkSyncEntry> NetworkSyncEntries = new HashSet<NetworkSyncEntry>();

        private MemoryCache _cache;
        private IDisposable _hearthbeatSubscriber;
        private ConcurrentDictionary<string, ConcurrentBag<InstanceMessage>> _messageCaches;

        public int Port { get; set; }
        public RSA PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Secret { get; set; }
        public ServerHeartbeat ServerHeartbeatLoop { get; set; } = new ServerHeartbeat();

        public Dictionary<string, DateTimeOffset> SyncedClientList { get; set; } = new Dictionary<string, DateTimeOffset>();
        public Dictionary<string, string> TokenClientIDMap { get; set; } = new Dictionary<string, string>();

        public string Url { get; set; }


        public SynapseNetworkServer()
        {
            _instance = this;
        }

        public static SynapseNetworkServer Instance => _instance ?? new SynapseNetworkServer();

        public WebServer Server { get; private set; }

        public WebServerState Status => Server?.State ?? WebServerState.Stopped;

        public void ServerHeartbeat()
        {
            var time = DateTimeOffset.Now;
            var pollRate = Synapse.Server.Get.Configs.synapseConfiguration.NetworkPollRate;
            var triplePoll = TimeSpan.FromMilliseconds(pollRate * 3);
            foreach (var pair in SyncedClientList.ToList())
            {
                var diff = time.Subtract(pair.Value);
                if (diff.CompareTo(triplePoll) == 1)
                {
                    Synapse.Server.Get.Logger.Warn($"Client {pair.Key} seems to be disconnected");
                    SyncedClientList.Remove(pair.Key);
                    _cache.Remove(pair.Key);
                }
            }
        }

        public void AddMessage(string receiver, InstanceMessage message)
        {
            var exists = _messageCaches.TryGetValue(receiver, out var messageList);
            if (!exists)
            {
                var bag = new ConcurrentBag<InstanceMessage>();
                bag.Add(message);
                _messageCaches.TryAdd(receiver, bag);
            }
            else
            {
                messageList.Add(message);
            }
        }

        public void AddClient(ClientData data)
        {
            TokenClientIDMap[data.SessionToken] = data.ClientUid;
            var options = new MemoryCacheEntryOptions
            {
                Size = 1,
                Priority = CacheItemPriority.Normal,
                SlidingExpiration =
                    TimeSpan.FromMilliseconds(
                        Math.Min(Synapse.Server.Get.Configs.synapseConfiguration.NetworkPollRate * 3, 60000))
            };
            options.RegisterPostEvictionCallback(EvictClient);
            _cache.Set(data.ClientUid, data, options);
        }

        public List<InstanceMessage> TakeAllMessages(ClientData data)
        {
            try
            {
                var found = _messageCaches.TryGetValue(data.ClientUid, out var messageBag);
                if (!found) return new List<InstanceMessage>();
                var list = new List<InstanceMessage>();
                while (!messageBag.IsEmpty)
                {
                    messageBag.TryTake(out var it);
                    list.Add(it);
                }

                return list;
            }
            catch (Exception e)
            {
                return new List<InstanceMessage>();
            }
        }

        private static void EvictClient(object key, object value, EvictionReason reason, object state)
        {
            Synapse.Server.Get.Logger.Warn("Evicted inactive ClientData & Messages");
            var clientDat = (ClientData) value;
            Instance._messageCaches.TryRemove(clientDat.ClientUid, out var ignored);
            Instance.TokenClientIDMap.Remove(clientDat.SessionToken);
        }

        public void Init()
        {
            Synapse.Server.Get.Logger.Info("Preparing Server");
            PrivateKey = RSA.Create();
            NetworkSyncEntries.Add(SerializableObjectWrapper.FromPair("example", "Some string value"));
            NetworkSyncEntries.Add(SerializableObjectWrapper.FromPair("example2", 6969));
            PublicKey = PrivateKey.ToXmlString(false);
            Synapse.Server.Get.Logger.Info("\n" + NetworkSyncEntries.Humanize());
            Synapse.Server.Get.Logger.Info("Init MemCache");
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 64
            });
            _messageCaches = new ConcurrentDictionary<string, ConcurrentBag<InstanceMessage>>();
            Synapse.Server.Get.Logger.Info("Done Init MemCache");
        }


        [CanBeNull]
        public ClientData DataById(string uid)
        {
            try
            {
                var exists = _cache.TryGetValue(uid, out var clientData);
                if (!exists) return null;
                return (ClientData) clientData;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [CanBeNull]
        public ClientData DataByToken(string sessionToken)
        {
            try
            {
                var user = TokenClientIDMap[sessionToken];
                if (user == null) return null;
                return DataById(user);
            }
            catch (Exception e)
            {
                return null;
            }
        }


        public void Start()
        {
            try
            {
                Synapse.Server.Get.Logger.Info("Setting up WebServer");
                Server = CreateWebServer(Url);
                Synapse.Server.Get.NetworkManager.NetworkNodes.ForEach(x => { x.RegisterWebserverWith(Server); });
#if DEBUG
                foreach (var module in Server.Modules)
                    Synapse.Server.Get.Logger.Info($"HTTP-Server Module with BaseRoute {module.BaseRoute} hooked");
#endif
                Synapse.Server.Get.Logger.Info("Executing WebServer");
                _hearthbeatSubscriber =
                    ServerHeartbeatLoop.HeartbeatSubject.Subscribe(new Consumer<object>(x => ServerHeartbeat()));
                ServerHeartbeatLoop.Start(true);
                Server.RunAsync();
            }
            catch (Exception e)
            {
                Synapse.Server.Get.Logger.Error(e);
            }
        }

        public void Stop()
        {
            Synapse.Server.Get.Logger.Info("Disposing Network Server...");
            Server?.Dispose();
            ServerHeartbeatLoop.Stop();
            _hearthbeatSubscriber.Dispose();
            _cache.Dispose();
            NetworkSyncEntries.Clear();
            NetworkSyncEntries.Add(SerializableObjectWrapper.FromPair("example", "Some string value"));
            NetworkSyncEntries.Add(SerializableObjectWrapper.FromPair("example2", 6969));
        }


        private static WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithWebApi("/synapse", m => m
                    .WithController<SynapseSynapseRouteController>()
                );


            // Listen for state changes.
            server.StateChanged += (s, e) => Logger.Get.Info($"WebServer New State - {e.NewState}");

            return server;
        }

        public static bool CheckServerPortAvailable(int port)
        {
            var address = Dns.GetHostEntry("localhost").AddressList[0];
            try
            {
                var tcpListener = new TcpListener(address, port);
                tcpListener.Start();
                tcpListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public interface IStatus
    {
        public string Message { get; set; }
        public bool Successful { get; set; }
    }

    public class StatusedResponse : IStatus
    {
        public static StatusedResponse Success = new SuccessfulStatus();
        public static StatusedResponse Unauthorized = new UnauthorizedStatus();
        public static StatusedResponse NotFound = new NotFoundStatus();

        public bool Successful { get; set; }
        public string Message { get; set; }
    }

    public class StatusListWrapper<T> : SuccessfulStatus, IEnumerable<T>
    {
        public StatusListWrapper(IEnumerable<T> enumerable)
        {
            Enumerable = enumerable;
        }

        private IEnumerable<T> Enumerable { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }
    }

    public class SuccessfulStatus : StatusedResponse
    {
        public SuccessfulStatus()
        {
            Successful = true;
            Message = "Ok";
        }
    }

    public class ErrorStatus : StatusedResponse
    {
        public ErrorStatus(string message)
        {
            Successful = false;
            Message = message;
        }
    }

    public class UnauthorizedStatus : ErrorStatus
    {
        public UnauthorizedStatus() : base("Unauthorized")
        {
        }
    }

    public class NotFoundStatus : ErrorStatus
    {
        public NotFoundStatus() : base("Not found")
        {
        }
    }
}