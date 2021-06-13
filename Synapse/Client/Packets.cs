using System;
using System.Text;
using Org.BouncyCastle.Utilities;
using Synapse.Client;
using UnityEngine;

namespace Synapse.Client.Packets
{
     public static class ConnectionSuccessfulPacket
    {
        public const ushort ID = 0;
    }
    
    public static class RoundStartPacket
    {
        public const ushort ID = 30;
    }

    public static class PositionPacket
    {
        public const ushort ID = 12;
        
        public static PipelinePacket Encode(Vector3 pos, Quaternion rot, string name)
        {
            var bytes = new byte[(4 * 3) + (4 * 4) + 4 + (4 + name.Length)];
            var xb = BitConverter.GetBytes(pos.x);
            var yb = BitConverter.GetBytes(pos.y);
            var zb = BitConverter.GetBytes(pos.z);
            var rwb = BitConverter.GetBytes(rot.w);
            var rxb = BitConverter.GetBytes(rot.x);
            var ryb = BitConverter.GetBytes(rot.y);
            var rzb = BitConverter.GetBytes(rot.z);
            var nlb = BitConverter.GetBytes(name.Length);
            var nb = Encoding.UTF8.GetBytes(name);
            for (var i = 0; i < 4; i++) bytes[i] = xb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 1)] = yb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 2)] = zb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 3)] = rwb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 4)] = rxb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 5)] = ryb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 6)] = rzb[i];
            for (var i = 0; i < 4; i++) bytes[i + (4 * 7)] = nlb[i];
            for (var i = 0; i < name.Length; i++) bytes[i + (4 * 8)] = nb[i];
            return PipelinePacket.From(ID, bytes);
        }

        public static void Decode(PipelinePacket packet, out Vector3 pos, out Quaternion rot, out string name)
        {
            var data = packet.AsByteArray();
            var x = BitConverter.ToSingle(data, 4 * 0);
            var y = BitConverter.ToSingle(data, 4 * 1);
            var z = BitConverter.ToSingle(data, 4 * 2);
            var rw = BitConverter.ToSingle(data, 4 * 3);
            var rx = BitConverter.ToSingle(data, 4 * 4);
            var ry = BitConverter.ToSingle(data, 4 * 5);
            var rz = BitConverter.ToSingle(data, 4 * 6);
            var nl = BitConverter.ToInt32(data, 4 * 7);
            var n = Encoding.UTF8.GetString(data, 4 * 8, nl);
            pos = new Vector3(x, y, z);
            rot = new Quaternion(rx, ry, rz, rw);
            name = n;
        }
    }

    public static class SpawnPacket
    {
        public const ushort ID = 10;
        public static PipelinePacket Encode(Vector3 pos, Quaternion rot, string name, string blueprint)
        {
            return PipelinePacket.From(ID, new Pack
            {
                Blueprint = blueprint,
                Name = name,
                x = pos.x,
                y = pos.y,
                z = pos.z,
                rx = rot.x,
                ry = rot.y,
                rz = rot.z,
                rw = rot.w
            });
        }

        public static void Decode(PipelinePacket packet, out Vector3 pos, out Quaternion rot, out string name, out string blueprint)
        {
            var pack = packet.As<Pack>();
            pos = new Vector3(pack.x, pack.y, pack.z);
            rot = new Quaternion(pack.rx, pack.ry, pack.rz, pack.rw);
            name = pack.Name;
            blueprint = pack.Blueprint;
        }

        private class Pack
        {
            public string Blueprint { get; set; }
            public string Name { get; set; }
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
            public float rx { get; set; }
            public float ry { get; set; }
            public float rz { get; set; }
            public float rw { get; set; }
        }
    }

    public static class DestroyPacket
    {
        public const ushort ID = 11;
        public static PipelinePacket Encode(string name, string blueprint)
        {
            return PipelinePacket.From(ID, new Pack
            {
                Name = name,
                Blueprint = blueprint
            });
        }

        public static void Decode(PipelinePacket packet, out string name, out string blueprint)
        {
            var pack = packet.As<Pack>();
            name = pack.Name;
            blueprint = pack.Blueprint;
        }

        private class Pack
        {
            public string Blueprint { get; set; }
            public string Name { get; set; }
        }
    }

    public static class RedirectPacket
    {
        public const ushort ID = 20;
        public static PipelinePacket Encode(string target)
        {
            return PipelinePacket.From(ID, new Pack
            {
                Target = target
            });
        }

        public static void Decode(PipelinePacket packet, out string target)
        {
            var pack = packet.As<Pack>();
            target = pack.Target;
        }
  
        private class Pack
        {
            public string Target { get; set; }
        }
    }

    public static class PlaySoundPacket
    {

        public const ushort ID = 21;
        public static PipelinePacket Encode(string name, Vector3 pos)
        {
            return PipelinePacket.From(ID, new Pack
            {
                Name = name,
                X = pos.x,
                Y = pos.y,
                Z = pos.z
            });
        }

        public static void Decode(PipelinePacket packet, out string name, out Vector3 pos)
        {
            var pack = packet.As<Pack>();
            name = pack.Name;
            pos = new Vector3(pack.X, pack.Y, pack.Z);
        }

        private class Pack
        {
            public string Name { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
        }
    }
}