using System;

namespace Synapse.Client
{
    public static class DataUtils
    {
        public static byte[] Pack(PipelinePacket packet)
        {
            var data = packet.Data;
            var buffer = new byte[data.Length + 7];
            buffer[0] = byte.MinValue;
            buffer[1] = byte.MaxValue;
            var idBytes = BitConverter.GetBytes(packet.PacketId);
            buffer[2] = idBytes[0];
            buffer[3] = idBytes[1];
            buffer[4] = idBytes[2];
            buffer[5] = idBytes[3];
            buffer[6] = packet.StreamStatus;
            for (var i = 0; i < data.Length; i++) buffer[i + 7] = data[i];
            return buffer;
        }
        
        public static PipelinePacket Unpack(byte[] encoded)
        {
            var packetId = BitConverter.ToUInt32(encoded, 2);
            var buffer = new byte[encoded.Length - 7];
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = encoded[i +  7];
            }
            return new PipelinePacket
            {
                PacketId = packetId,
                Data = buffer,
                StreamStatus = encoded[6]
            };
        }

        public static bool IsData(byte[] bytes)
        {
            if (bytes.Length < 2) return false;
            return bytes[0] == byte.MinValue && bytes[1] == byte.MaxValue;
        }

    }
}