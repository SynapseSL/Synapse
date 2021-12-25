using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.Api.Enum
{
    public enum DamageTranslation : byte
    {
        Recontained = 0,
        Nuck = 1,
        Scp049 = 2,
        Unknown = 3,
        Asphyxiated = 4,
        Bleeding = 5,
        Falldown = 6,
        PocketDecay = 7,
        Decontamination = 8,
        Poisoned = 9,
        Scp207 = 10,
        SeveredHands = 11,
        MicroHID = 12,
        Tesla = 13,
        Explosion = 14,
        Scp096 = 15,
        Scp173 = 16,
        Scp939 = 17,
        Zombie = 18,
        BulletWounds = 19,
        Crushed = 20,
        UsedAs106Bait = 21,
        FriendlyFireDetector = 22,
        Hypothermia = 23,
        None = 100, // In cant use the -1 beacause it was a byte
    }
}
