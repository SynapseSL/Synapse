using Neuron.Core.Meta;
using Synapse3.SynapseModule.Map.Rooms;

namespace Synapse3.SynapseModule.Map;

public class HeavyZoneService : Service
{
    private readonly RoomService _room;

    public HeavyZoneService(RoomService room)
    {
        _room = room;
    }

    private  Recontainer079 Recontainer => Synapse.GetObjectOf<Recontainer079>();

    public byte ActiveGenerators => (byte)Recontainer._prevEngaged;

    /// <summary>
    /// True if SCP079 has been recontained
    /// </summary>
    public bool Is079Recontained
    {
        get
        {
            var recontainer = Recontainer;
            return Recontainer._alreadyRecontained && Recontainer._delayStopwatch.Elapsed.TotalSeconds > recontainer._activationDelay;
        }
    }

    /// <summary>
    /// Recontain SCP079 manually
    /// </summary>
    public void Recontain079()
    {
        var recontainer = Recontainer;

        recontainer.TryKill079();
        recontainer.PlayAnnouncement(recontainer._announcementSuccess + " Unknown", 1f);
    }

    /// <summary>
    /// Trigger Heavy Containment Zone overcharge manually 
    /// </summary>
    public void Overcharge() => Recontainer.Recontain();

    /// <summary>
    /// Turn off all lights (in the HCZ)
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="onlyHeavy"></param>
    public void LightsOut(float duration, bool onlyHeavy = true)
    {
        foreach (var room in _room.Rooms)
        {
            if (!onlyHeavy || (ZoneType)room.Zone == ZoneType.HCZ)
            {
                room.TurnOffLights(duration);
            }
        }
    }
}