namespace Synapse3.SynapseModule.Player;

public class Scp096Controller
{
    
    private readonly SynapsePlayer _player;
    
    internal Scp096Controller(SynapsePlayer player)
    {
        _player = player;
    }
    /*
    
    public Scp096 Scp096 => _player.Hub.scpsController.CurrentScp as Scp096;
    public bool Is096Instance => Scp096 != null;
    
    public float ShieldAmount
    {
        get
        {
            if (Is096Instance) return Scp096.ShieldAmount;
            return 0;
        }
        set
        {
            if (!Is096Instance) return;
            Scp096.ShieldAmount = value;
        }
    }
    
    public float MaxShield { get; set; } = 350f;

    public float CurMaxShield
    {
        get
        {
            if (Is096Instance) return Scp096.CurMaxShield;
            return 0f;
        }
        set
        {
            if (!Is096Instance) return;
            Scp096.CurMaxShield = value;
        }
    }

    public float EnrageTimeLeft
    {
        get
        {
            if (Is096Instance) return Scp096.EnrageTimeLeft;
            return 0f;
        }
        set
        {
            if (!Is096Instance) return;
            Scp096.EnrageTimeLeft = value;
        }
    }

    public Scp096PlayerState RageState
    {
        get
        {
            if (Is096Instance) return Scp096.PlayerState;
            return Scp096PlayerState.Docile;
        }
        set
        {
            if (!Is096Instance) return;
             switch (value)
                {
                    case Scp096PlayerState.Charging:
                        if (RageState != Scp096PlayerState.Enraged)
                            RageState = Scp096PlayerState.Enraged;
                        Scp096.Charge();
                        break;

                    case Scp096PlayerState.Calming:
                        Scp096.EndEnrage();
                        break;

                    case Scp096PlayerState.Enraged when RageState != Scp096PlayerState.Attacking:
                        if (RageState == Scp096PlayerState.Docile
                            || RageState == Scp096PlayerState.TryNotToCry
                            || RageState == Scp096PlayerState.Calming)
                            RageState = Scp096PlayerState.Enraging;
                        Scp096.Enrage();
                        break;

                    case Scp096PlayerState.Enraged when RageState == Scp096PlayerState.Attacking:
                        Scp096.EndAttack();
                        break;

                    case Scp096PlayerState.TryNotToCry:
                        if (RageState != Scp096PlayerState.Docile)
                            RageState = Scp096PlayerState.Docile;
                        Scp096.TryNotToCry();
                        break;

                    case Scp096PlayerState.Attacking:
                        if (RageState != Scp096PlayerState.Enraged)
                            RageState = Scp096PlayerState.Enraged;
                        Scp096.ServerDoAttack(_player.Connection, default);
                        break;

                    case Scp096PlayerState.Enraging:
                        if (RageState != Scp096PlayerState.Docile)
                            RageState = Scp096PlayerState.Docile;
                        Scp096.Windup();
                        break;

                    case Scp096PlayerState.Docile:
                        Scp096.ResetEnrage();
                        break;

                        //Since you also need a Door for PryGate is it not supported by this and you have to use ChargeDoor()
                }
        }
    }

    public List<SynapsePlayer> Targets
    {
        get
        {
            if (!Is096Instance) return new List<SynapsePlayer>();
            return Scp096._targets.Select(x => x.GetSynapsePlayer()).ToList();
        }
    }

    public bool CanAttack
    {
        get
        {
            if (Is096Instance) return Scp096.CanAttack;
            return false;
        }
    }

    public bool CanCharge
    {
        get
        {
            if (Is096Instance) return Scp096.CanCharge;
            return false;
        }
    }

    public void AddTarget(SynapsePlayer player)
    {
        if (!Is096Instance || !Scp096.CanReceiveTargets) return;

        Scp096.AddTarget(player.gameObject);
    }

    public void RemoveTarget(SynapsePlayer player)
    {
        if (!Is096Instance) return;

        Scp096._targets.Remove(player.Hub);
    }

    public void ChargeDoor(SynapseDoor door)
    {
        if (!Is096Instance) return;
        Scp096.ChargeDoor(door.Variant);
    }
    */
}