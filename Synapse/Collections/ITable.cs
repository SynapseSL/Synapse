using System.Collections.Generic;

namespace Synapse.Collections
{
    public interface ITable<TA, TB, TC>
    {
        bool TryGet(TA x, TB y, out TC result);
        bool TryGet(TA x, out IDictionary<TB, TC> result);

        bool TrySet(TA x, TB y, TC value);

        bool TryRemove(TA x, TB y);
        bool TryRemove(TA x);

        bool ContainsKey(TA x);
        bool ContainsKey(TA x, TB y);

        ICollection<TA> Keys();

        int Count();
        int Count(TA x);

        void Clear();
    }
}