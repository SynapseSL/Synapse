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
        
        public static void receive(Player player, PipelinePacket data)
        {
            Logger.Get.Info($"=pipeline=>  {data.ToString()}");
            DataReceivedEvent?.Invoke(player, data);
        }

        public static void invokeConnectionComplete(Player player)
        {
            var con = ClientManager.Singleton.Clients[player.UserId];
            ClientConnectionCompleteEvent?.Invoke(player, new ClientConnectionComplete()
            {
                Data = con,
                Player = player
            });
        }
        
        public static void invoke(Player player, PipelinePacket packet)
        {
            var packed = DataUtils.pack(packet);
            Logger.Get.Info($"<=pipeline=  {Base64.ToBase64String(packed)}"); 
            player.GameConsoleTransmission.TargetPrintOnConsole(player.Connection, packed, false);
        }
        
        public static void invokeBroadcast(PipelinePacket packet)
        {
            foreach (var player in SynapseController.Server.Players)
            {
                invoke(player, packet);
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
        public ushort PacketId { get; set; }
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

        public static PipelinePacket from(ushort id, byte[] payload)
        {
            return new PipelinePacket
            {
                PacketId = id,
                Data = payload
            };
        }
        
        public static PipelinePacket from(ushort id, string payload)
        {
            return new PipelinePacket
            {
                PacketId = id,
                Data = Encoding.UTF8.GetBytes(payload)
            };
        }

        public static PipelinePacket from<T>(ushort id, T payload)
        {
            var encoded = JsonConvert.SerializeObject(payload);
            return from(id, encoded);
        }
    }

    public class ClientConnectionComplete
    {
        public ClientConnectionData Data { get; set; }
        public Player Player { get; set; }
    }
}