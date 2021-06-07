using System.Collections.Generic;

namespace Synapse.Collections
{
    public class Table<TA, TB, TC> : ITable<TA, TB, TC>
    {
        private readonly Dictionary<TA, Dictionary<TB, TC>> _dictionary = new();

        public bool TryGet(TA x, TB y, out TC result)
        {
            var b1 = _dictionary.TryGetValue(x, out var yDict);
            if (!b1)
            {
                result = default;
                return false;
            }

            var b2 = yDict.TryGetValue(y, out var z);
            result = z;
            return b2;
        }

        public bool TryGet(TA x, out IDictionary<TB, TC> result)
        {
            var b1 = _dictionary.TryGetValue(x, out var yDict);
            if (!b1)
            {
                result = default;
                return false;
            }

            result = yDict;
            return true;
        }

        public bool TrySet(TA x, TB y, TC value)
        {
            var b1 = _dictionary.TryGetValue(x, out var yDict);
            if (!b1)
            {
                yDict = new Dictionary<TB, TC>();
                _dictionary.Add(x, yDict);
            }

            if (yDict.ContainsKey(y)) return false;
            yDict[y] = value;
            return true;
        }

        public bool TryRemove(TA x, TB y)
        {
            var b1 = _dictionary.TryGetValue(x, out var yDict);
            if (!b1) return false;
            if (!yDict.Remove(y)) return false;
            if (yDict.Count == 0) _dictionary.Remove(x);
            return true;
        }

        public bool TryRemove(TA x)
        {
            return _dictionary.Remove(x);
        }

        public bool ContainsKey(TA x)
        {
            return _dictionary.ContainsKey(x);
        }

        public bool ContainsKey(TA x, TB y)
        {
            var b1 = _dictionary.TryGetValue(x, out var yDict);
            if (!b1) return false;
            return yDict.ContainsKey(y);
        }

        public ICollection<TA> Keys()
        {
            return _dictionary.Keys;
        }

        public int Count()
        {
            return _dictionary.Count;
        }

        public int Count(TA x)
        {
            var b1 = _dictionary.TryGetValue(x, out var yDict);
            return !b1 ? 0 : yDict.Count;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }
    }
}