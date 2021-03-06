using System.Collections.Generic;

namespace Synapse.Collections
{
    public interface ICache<TA, TB, TC>
    {
        bool TryPeek(TA x, TB y, out TC result);
        bool TryPeek(TA x, out IDictionary<TB, TC> result);
    }
}