using System;
using Swan.Formatters;

namespace Synapse.Network
{
    [Serializable]
    public class SerializableObjectWrapper
    {
        private object _valStore;
        public string Class { get; set; }
        public string Data { get; set; }

        [JsonProperty("_", true)] public Type ParseTypeData => Type.GetType(Class);

        [JsonProperty("__", true)]
        public bool IsCoreType => ParseTypeData?.AssemblyQualifiedName?.Contains("mscorlib") ?? true;

        public static bool CheckIsCoreType(Type type)
        {
            return type?.AssemblyQualifiedName?.Contains("mscorlib") ?? true;
        }

        public T Value<T>()
        {
            if (_valStore != null) return (T) _valStore;
            var val = Parse();
            _valStore = val;
            return (T) val;
        }

        public object Value()
        {
            if (_valStore != null) return _valStore;
            var val = Parse();
            _valStore = val;
            return val;
        }

        public void Update<T>(T obj)
        {
            _valStore = null;
            Data = Serialize(obj);
            Class = obj.GetType().AssemblyQualifiedName;
        }

        public static NetworkSyncEntry FromPair<T>(string key, T value)
        {
            var type = value.GetType();
            return new NetworkSyncEntry
            {
                Key = key,
                Data = Serialize(value),
                Class = type.AssemblyQualifiedName.Contains("mscorlib") ? type.Name : type.AssemblyQualifiedName
            };
        }

        public object Parse()
        {
            var t = ParseTypeData;
            if (IsCoreType)
            {
#if DEBUG
                Server.Get.Logger.Info("Primitive DataType Deserialization");
#endif
                if (t == typeof(string)) return Data;
                if (t == typeof(int)) return int.Parse(Data);
                if (t == typeof(float)) return float.Parse(Data);
                if (t == typeof(bool)) return bool.Parse(Data);
                if (t == typeof(long)) return long.Parse(Data);
                if (t == typeof(double)) return double.Parse(Data);
                if (t == typeof(short)) return short.Parse(Data);
                if (t == typeof(byte)) return byte.Parse(Data);
            }

            return Json.Deserialize(Data, t);
        }

        public static string Serialize(object obj)
        {
            var t = obj.GetType();
            if (CheckIsCoreType(t))
            {
#if DEBUG
                Server.Get.Logger.Info("Primitive DataType Serialization");
#endif
                return obj.ToString();
            }

            return Json.Serialize(obj);
        }
    }
}