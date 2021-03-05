using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using EmbedIO;
using EmbedIO.WebApi;
using JetBrains.Annotations;
using Swan;
using Swan.Logging;

namespace Synapse.Network
{
    public class SynapseNetworkServer
    {

        private static SynapseNetworkServer _instance;
        public static SynapseNetworkServer Instance => _instance ?? new SynapseNetworkServer();

        public WebServer Server { get; private set; }

        public string Url;
        public string Secret;
        public int Port;
        
        public readonly string PublicKey;
        public readonly RSA PrivateKey;
        public readonly List<ClientData> ClientData = new List<ClientData>();       
        public readonly HashSet<NetworkSyncEntry> NetworkSyncEntries = new HashSet<NetworkSyncEntry>();

        public WebServerState Status => Server?.State ?? WebServerState.Stopped;

        public SynapseNetworkServer()
        {
            _instance = this;
            PrivateKey = RSA.Create();
            NetworkSyncEntries.Add(NetworkSyncEntry.FromPair("example", "Some string value"));
            NetworkSyncEntries.Add(NetworkSyncEntry.FromPair("example2", 6969));
            PublicKey = PrivateKey.ToXmlString(false);
            Synapse.Server.Get.Logger.Info("\n" + NetworkSyncEntries.Humanize());
        }
        
        public ClientData DataById(string uid)
        {
            return ClientData.First(x => x.ClientUid == uid);
        }
        
        [CanBeNull]
        public ClientData DataByToken(string sessionToken)
        {
            var values = ClientData.Where(x => x.SessionToken == sessionToken).ToList();
            return values.IsEmpty() ? null : values.First();
        }


        public void Start()
        {
            try
            {
                Synapse.Server.Get.Logger.Info("Setting up WebServer");
                Server = CreateWebServer(Url);
                Synapse.Server.Get.NetworkManager.NetworkNodes.ForEach(x =>
                {
                    x.RegisterWebserverWith(Server);
                });
                #if DEBUG
                foreach (var module in Server.Modules)
                {
                    Synapse.Server.Get.Logger.Info($"HTTP-Server Module with BaseRoute {module.BaseRoute} hooked");
                }
                #endif
                Synapse.Server.Get.Logger.Info("Executing WebServer");
                Server.RunAsync();
            }
            catch (Exception e)
            {
                Synapse.Server.Get.Logger.Error(e);
            }
        }

        public void Stop()
        {
            Server?.Dispose();
            ClientData.Clear();          
            NetworkSyncEntries.Clear();
            NetworkSyncEntries.Add(NetworkSyncEntry.FromPair("example", "Some string value"));
            NetworkSyncEntries.Add(NetworkSyncEntry.FromPair("example2", 6969));
        }
        
            
        // Create and configure our web server.
        private static WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithWebApi("/synapse", m => m
                    .WithController<SynapseSynapseRouteController>()
                );
            // .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));
            
            
            // Listen for state changes.
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

            return server;
        }

        public static long CurrentMillis()
        {
            return new DateTime().Millisecond;
        }
        
        public static bool CheckServerPortAvailable(int port)
        {
            var address = Dns.GetHostEntry("localhost").AddressList[0];
            try
            {
                TcpListener tcpListener = new TcpListener(address, port);
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
    
    public enum InstanceAuthority
    {
        Master,
        Client
    }

    public class PingResponse : SuccessfulStatus
    {
        public bool Authenticated { get; set; }
    }
    
    public class StatusedResponse
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

        private IEnumerable<T> Enumerable { get; set; }
        
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