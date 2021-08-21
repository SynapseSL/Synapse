using System;

namespace Synapse.Api.Enum
{
    public enum GeneratorInteraction
    {
        Activated,
        Disabled,
        Unlocked,
        OpenDoor,
        CloseDoor,

        [Obsolete("Use Activated",true)]
        TabletInjected = 0,
        [Obsolete("Use Disabled",true)]
        TabledEjected = 1,
    }
}
