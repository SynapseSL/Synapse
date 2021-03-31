using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Swan.Formatters;

namespace Synapse.Network.Models
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
            if (type == null || type.AssemblyQualifiedName == null) return false;
            if (type.AssemblyQualifiedName.Contains("mscorlib")) return type.IsPrimitive;
            return false;
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

        public static KeyValueObjectWrapper FromPair<T>(string key, T value)
        {
            var type = value.GetType();
            return new KeyValueObjectWrapper
            {
                Key = key,
                Data = Serialize(value),
                Class = type.AssemblyQualifiedName.Contains("mscorlib") ? type.Name : type.AssemblyQualifiedName
            };
        }

        public static NetworkSyncEntry NetFromPair<T>(string key, T value)
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

    public class KeyValueObjectWrapper : SerializableObjectWrapper
    {
        public string Key { get; set; }
    }

    internal static class SOWExtension
    {
        public static List<string> Keys(this HashSet<KeyValueObjectWrapper> set)
        {
            return set.Select(x => x.Key).ToList();
        }

        [CanBeNull]
        public static object Get(this HashSet<KeyValueObjectWrapper> set, string key)
        {
            var list = set.Where(x => x.Key == key).ToList();
            return list.IsEmpty() ? null : list[0];
        }

        [CanBeNull]
        public static T Get<T>(this HashSet<KeyValueObjectWrapper> set, string key)
        {
            return (T) set.Get(key);
        }

        public static void Set(this HashSet<KeyValueObjectWrapper> set, string key, object obj)
        {
            var list = set.Where(x => x.Key == key).ToList();
            if (!list.IsEmpty()) set.Remove(list[0]);
            set.Add(SerializableObjectWrapper.FromPair(key, obj));
        }
    }
}