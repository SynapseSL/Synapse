using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Map;

public class HeavyZoneService : Service
{
    //TODO:
    /*
    private readonly RoomService _room;
    private readonly NukeService _nuke;

    public HeavyZoneService(RoomService room, NukeService nuke)
    {
        _room = room;
        _nuke = nuke;
    }

    
    private Recontainer079 ReContainer => Synapse.GetObject<Recontainer079>();

    public byte ActiveGenerators => (byte)ReContainer._prevEngaged;

    /// <summary>
    /// True if SCP079 has been contained
    /// </summary>
    public bool Is079Contained
    {
        get
        {
            var reContainer = ReContainer;
            //TODO: Rework this when the 079 Soft Rework was released
            return (reContainer._alreadyRecontained &&
                    reContainer._delayStopwatch.Elapsed.TotalSeconds > reContainer._activationDelay) ||
                   _nuke.State == NukeState.Detonated;
        }
    }

    /// <summary>
    /// Contain SCP079 manually instant and without any Power outages
    /// </summary>
    public void Contain079()
    {
        var reContainer = ReContainer;
        if (!reContainer.TryKill079()) return;
        
        reContainer.PlayAnnouncement(reContainer._announcementFailure, 1f);
        new SubtitleMessage(new[]
        {
            new SubtitlePart(SubtitleType.OperationalMode, null)
        }).SendToAuthenticated();
    }

    /// <summary>
    /// Trigger Heavy Containment Zone overcharge manually 
    /// </summary>
    public void Overcharge() => ReContainer.Recontain();

    /// <summary>
    /// Turn off all lights (in the HCZ)
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="onlyHeavy"></param>
    public void LightsOut(float duration, bool onlyHeavy = true)
    {
        foreach (var room in _room.Rooms)
        {
            if (!onlyHeavy || (ZoneType)room.Zone == ZoneType.Hcz)
            {
                room.TurnOffLights(duration);
            }
        }
    }
    */
}