using Newtonsoft.Json;
using Synapse.RCE.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Synapse.RCE
{
    internal class SynapseRceServer
    {
        private readonly RoslynCompiler _roslynCompiler;
        private readonly TcpListener _listener;
        private readonly JsonSerializerSettings _serializerSettings;

        internal SynapseRceServer(IPAddress address, int port)
        {
            _roslynCompiler = new();
            _listener = new(address, port);
            _serializerSettings = new() { MissingMemberHandling = MissingMemberHandling.Error };
        }

        internal void Start()
        {
            Server.Get.Logger.Info("Starting RCE-Server...");
            _listener.Start(1);
            Task.Run(ListenForClient);
        }
        internal void Stop()
        {
            Server.Get.Logger.Info("Stopping RCE-Server...");
            _listener.Stop();
        }
        private void ListenForClient()
        {
            try
            {
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception e) when (e is IOException || e is SocketException)
            {
                Api.Logger.Get.Error($"RCE-Server Connection forcibly closed");
                // Connection forcibly closed, ignore and run out
            }
            catch (Exception e)
            {
                Api.Logger.Get.Error($"LocalCommunicationHandler Exception: {e}");
            }
        }
        private void HandleClient(TcpClient client)
        {
            using var clientNetStream = client.GetStream();
            using var netStreamWriter = new StreamWriter(clientNetStream);
            netStreamWriter.AutoFlush = true;
            using var netStreamReader = new StreamReader(clientNetStream);

            while (!netStreamReader.EndOfStream)
            {
                try
                {
                    var strContent = netStreamReader.ReadLine();
                    var rceRequest = JsonConvert.DeserializeObject<RceRequest>(strContent, _serializerSettings);

                    var methodInfo = _roslynCompiler.TryCompile(rceRequest, out var failResponse);
                    if (methodInfo is null)
                    {
                        netStreamWriter.WriteLine(JsonConvert.SerializeObject(failResponse));
                        client.Dispose();
                        return;
                    }

                    // Prepare object to be handed over to concurrent Unity-Context
                    void action() => methodInfo.Invoke(null, new object[] { new string[0] });
                    var qAction = new QueueAction()
                    {
                        Action = action,
                        Exception = null,
                        Ran = false
                    };

                    // Send to concurrent Unity-Context, which dequeues and executes regularly
                    Server.Get.RceHandler.ActionQueue.Enqueue(qAction);

                    // Wait until qAction has been executed
                    while (!qAction.Ran) ;

                    // Choose response depending on whether it failed or not
                    RceResponse response = qAction.Exception is null
                        ? RceResponse.GetSuccessResponse()
                        : RceResponse.GetFailedBuildResponse(qAction.Exception);

                    netStreamWriter.WriteLine(JsonConvert.SerializeObject(response));
                }
                catch (JsonSerializationException e)
                {
                    var response = RceResponse.GetInvalidJsonResponse();
                    var responseJson = JsonConvert.SerializeObject(response);
                    netStreamWriter.WriteLine(responseJson);
                    Api.Logger.Get.Debug($"RCE-Server Json Serialization Error: {e}");
                }
                catch (Exception e) when (e is IOException || e is SocketException)
                {
                    Api.Logger.Get.Error("RCE-Server Connection forcibly closed");
                    // Connection forcibly closed, ignore and run out
                    break;
                }
                catch (Exception e)
                {
                    Api.Logger.Get.Error($"LocalCommunicationHandler Exception: {e}");
                }
            }
        }
    }
}