using System.Linq;
using MEC;

namespace Synapse3.SynapseModule.Player;

public class Broadcast
{
    public float DisplayTime { get; internal set; } = float.MinValue;
    public ushort Time { get; }
    public bool Active { get; private set; }
    private readonly SynapsePlayer _player;
    private string msg;

    private string Message
    {
        get => msg;
        set
        {
            if (value != msg)
            {
                msg = value;
                if (Active)
                    Refresh();
            }
        }
    }

    public Broadcast(string msg, ushort time, SynapsePlayer player)
    {
        Message = msg;
        Time = time;
        _player = player;
    }

    public void StartBc(SynapsePlayer player)
    {
        if (player.ActiveBroadcasts.FirstOrDefault() != this)
            return;

        if (Active)
            return;

        Active = true;
        DisplayTime = UnityEngine.Time.time;
        _player.Broadcast(Time, Message);
        Timing.CallDelayed(Time, () => EndBc());
    }

    public void Refresh()
    {
        float time = Time - (UnityEngine.Time.time - DisplayTime) + 1;
        _player.InstantBroadcast((ushort) time, Message);
    }

    public void EndBc()
    {
        if (!Active)
            return;

        Active = false;
        _player.ActiveBroadcasts.Remove(this);
        _player.ClearBroadcasts();
        
        if(_player.ActiveBroadcasts.FirstOrDefault() != null)
            _player.ActiveBroadcasts.FirstOrDefault().StartBc(_player);
    }
}