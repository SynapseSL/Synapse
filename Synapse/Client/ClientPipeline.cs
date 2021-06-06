using System;
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
        
        public static void receive(Player player, PipelinePacket data)
        {
            Logger.Get.Info($"=pipeline=>  {data.ToString()}");
            DataReceivedEvent?.Invoke(player, data);
        }

        public static void invoke(Player player, PipelinePacket packet)
        {
            var packed = DataUtils.pack(packet);
            Logger.Get.Info($"<=pipeline=  {Base64.ToBase64String(packed)}"); 
            player.GameConsoleTransmission.TargetPrintOnConsole(player.Connection, packed, false);
        }

        public delegate void DataEvent<in TEvent>(Player player, TEvent ev);
    }
    
 public static class DataUtils
    {
        public static byte[] pack(PipelinePacket packet)
        {
            var data = packet.Data;
            var buffer = new byte[data.Length + 5];
            buffer[0] = byte.MinValue;
            buffer[1] = byte.MaxValue;
            var idBytes = BitConverter.GetBytes(packet.PacketId);
            buffer[2] = idBytes[0];
            buffer[3] = idBytes[1];
            buffer[4] = packet.StreamStatus;
            for (var i = 0; i < data.Length; i++) buffer[i + 5] = data[i];
            return buffer;
        }
        
        public static PipelinePacket unpack(byte[] encoded)
        {
            var packetId = BitConverter.ToUInt16(encoded, 2);
            var buffer = new byte[encoded.Length - 5];
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = encoded[i +  5];
            }
            return new PipelinePacket
            {
                PacketId = packetId,
                Data = buffer,
                StreamStatus = encoded[4]
            };
        }

        public static bool isData(byte[] bytes)
        {
            if (bytes.Length < 2) return false;
            return bytes[0] == byte.MinValue && bytes[1] == byte.MaxValue;
        }

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

        public static PipelinePacket from(uint id, byte[] payload)
        {
            return new PipelinePacket
            {
                PacketId = id,
                Data = payload
            };
        }
        
        public static PipelinePacket from(uint id, string payload)
        {
            return new PipelinePacket
            {
                PacketId = id,
                Data = Encoding.UTF8.GetBytes(payload)
            };
        }

        public static PipelinePacket from<T>(uint id, T payload)
        {
            var encoded = JsonConvert.SerializeObject(payload);
            return from(id, encoded);
        }
    }
}