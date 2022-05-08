using Newtonsoft.Json;
using Synapse.RCE.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Synapse.RCE
{
    public class SynapseRceClient
    {
        public int Port { get; }

        public SynapseRceClient(int port)
        {
            Port = port;
        }

        public RceResponse ExecuteFromFile(string path, string assemblyName = null, int? timeout = null)
        {
            var code = File.ReadAllText(path);
            return ExecuteFromCode(code, assemblyName, timeout);
        }
        public async Task<RceResponse> ExecuteFromFileAsync(string path, string assemblyName = null, int? timeout = null)
        {
            var code = File.ReadAllText(path);
            return await ExecuteFromCodeAsync(code, assemblyName);
        }
        public RceResponse ExecuteFromCode(string code, string assemblyName = null, int? timeout = null)
        {
            RceResponse retVal = null;
            assemblyName ??= Guid.NewGuid().ToString();
            try
            {
                using TcpClient client = new TcpClient();
                client.Connect(IPAddress.Loopback, Port);
                if (timeout is { } timeoutVal)
                {
                    client.SendTimeout = timeoutVal;
                    client.ReceiveTimeout = timeoutVal;
                }

                using var streamWriter = new StreamWriter(client.GetStream());
                streamWriter.AutoFlush = true;
                using var streamReader = new StreamReader(client.GetStream());

                var request = new RceRequest()
                {
                    AssemblyName = assemblyName,
                    Code = code
                };
                var requestStr = JsonConvert.SerializeObject(request);
                streamWriter.WriteLine(requestStr);

                var responseStr = streamReader.ReadLine();
                retVal = JsonConvert.DeserializeObject<RceResponse>(responseStr);
            }
            catch (Exception e) when (e is SocketException || e is IOException)
            {
                retVal = new RceResponse()
                {
                    Status = RceStatus.ConnectionFailed,
                    Content = $"Failed to Connect with Server. {e}"
                };
            }
            return retVal;
        }
        public async Task<RceResponse> ExecuteFromCodeAsync(string code, string assemblyName = null, int? timeout = null)
        {
            RceResponse retVal = null;
            assemblyName ??= Guid.NewGuid().ToString();
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, Port);
                if (timeout is { } timeoutVal)
                {
                    client.SendTimeout = timeoutVal;
                    client.ReceiveTimeout = timeoutVal;
                }

                using var streamWriter = new StreamWriter(client.GetStream());
                streamWriter.AutoFlush = true;
                using var streamReader = new StreamReader(client.GetStream());

                var request = new RceRequest()
                {
                    AssemblyName = assemblyName,
                    Code = code
                };
                var requestStr = JsonConvert.SerializeObject(request);
                await streamWriter.WriteLineAsync(requestStr);

                var responseStr = await streamReader.ReadLineAsync();
                retVal = JsonConvert.DeserializeObject<RceResponse>(responseStr);
            }
            catch (Exception e) when (e is SocketException || e is IOException)
            {
                retVal = new RceResponse()
                {
                    Status = RceStatus.ConnectionFailed,
                    Content = $"Failed to Connect with Server. {e}"
                };
            }
            return retVal;
        }
    }
}
