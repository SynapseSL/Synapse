using System;

namespace Synapse.Client
{
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
}