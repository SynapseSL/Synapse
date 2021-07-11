using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;
using Swan;
using Synapse.Api;

namespace Synapse.Client
{
    public static class ClientPipeline
    {
        public static event DataEvent<PipelinePacket> DataReceivedEvent;
        public static event DataEvent<ClientConnectionComplete> ClientConnectionCompleteEvent;
        
        public static void Receive(Player player, PipelinePacket data)
        {
            Logger.Get.Info($"=pipeline=> {data}");
            DataReceivedEvent?.Invoke(player, data);
        }

        public static void InvokeConnectionComplete(Player player)
        {
            var con = SynapseController.ClientManager.Clients[player.UserId];
            ClientConnectionCompleteEvent?.Invoke(player, new ClientConnectionComplete()
            {
                Data = con,
                Player = player
            });
        }
        
        public static void Invoke(Player player, PipelinePacket packet)
        {
            var packed = DataUtils.Pack(packet);
            Logger.Get.Info($"<=pipeline=  {Base64.ToBase64String(packed)}"); 
            player.GameConsoleTransmission.TargetPrintOnConsole(player.Connection, packed, false);
        }
        
        public static void InvokeBroadcast(PipelinePacket packet)
        {
            foreach (var player in SynapseController.Server.Players)
            {
                Invoke(player, packet);
            }
        }

        public delegate void DataEvent<in TEvent>(Player player, TEvent ev);
    }

    /*
     == PacketList ==
     
     BaseMisc:
     0: Welcome Packet
     1: Message Packet
     
     Object Packets
     10: Object Spawn
     11: Object Destroy
     12: Object Location
     
     Client Packets
     20: Client Redirect
     21: Client PlaySound
     
     Streaming Packets
     30: Streamed AssetBundle
    
     */
    
    public class PipelinePacket
    {
        public uint PacketId { get; set; }
        public byte[] Data { get; set; }
        public byte StreamStatus { get; set; } = 0x00;
 
        public override string ToString()
        {
            return $"Packet_{PacketId} [ {Base64.ToBase64String(Data)} ] Stream: {StreamStatus}";
        }

        public string AsString()
        {
            return Encoding.UTF8.GetString(Data);
        }

        //Just to maintain a style
        public byte[] AsByteArray()
        {
            return Data;
        }

        public T As<T>()
        {
            var s = AsString();
            return JsonConvert.DeserializeObject<T>(s);
        }

        public static PipelinePacket From(uint id, byte[] payload)
        {
            return new PipelinePacket
            {
                PacketId = id,
                Data = payload
            };
        }
        
        public static PipelinePacket From(uint id, string payload)
        {
            return new PipelinePacket
            {
                PacketId = id,
                Data = Encoding.UTF8.GetBytes(payload)
            };
        }

        public static PipelinePacket From<T>(uint id, T payload)
        {
            var encoded = JsonConvert.SerializeObject(payload);
            return From(id, encoded);
        }
    }

    public class ClientConnectionComplete
    {
        public ClientConnectionData Data { get; set; }
        public Player Player { get; set; }
    }
}