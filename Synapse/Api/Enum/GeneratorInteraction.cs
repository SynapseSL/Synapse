namespace Synapse.Api.Enum
{
    public enum GeneratorInteraction
    {
        Activated,
        Disabled,
        Unlocked,
        OpenDoor,
        CloseDoor,


        [System.Obsolete("Use Activated", true)]
        TabletInjected = 0,
        [System.Obsolete("Use Disabled", true)]
        TabledEjected = 1,
    }
}