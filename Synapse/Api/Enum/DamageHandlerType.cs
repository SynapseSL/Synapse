using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Api.Enum
{
    public enum DamageHandlerType : long
    {
        Unknown = 0,
        Standard = 0b_0000_0000_0001,
        Universal = 0b_0000_0000_0011,
        Attacker = 0b_0000_0000_0101,
        CustomReason = 0b_0000_0000_1001,
        Nuck = 0b_0000_0000_1001,
        Scp = 0b_0000_0001_0101,
        Firearm = 0b_0000_0010_0101,
        Scp096 = 0b_0000_0100_0101,
        Explosion = 0b_0000_1000_0101,
        MicroHid = 0b_0001_0000_0101,
        Recontainment = 0b_0010_0000_0101,
        Scp018 = 0b_0100_0000_0101,
        Disruptor = 0b_1000_0000_0101,
    }
}
