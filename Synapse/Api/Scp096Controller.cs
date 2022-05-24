using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api
{
    public class Scp096Controller
    {
        internal Scp096Controller(Player _player) => player = _player;

        private readonly Player player;

        private PlayableScps.Scp096 Scp096 => player.Hub.scpsController.CurrentScp as PlayableScps.Scp096;

        public bool Is096 => player.RoleType == RoleType.Scp096;

        public float ShieldAmount
        {
            get => Is096 ? Scp096.ShieldAmount : 0;
            set
            {
                if (!Is096)
                    return;
                Scp096.ShieldAmount = value;
            }
        }

        public float MaxShield { get; set; } = 350f;

        public float CurMaxShield
        {
            get => Is096 ? Scp096.CurMaxShield : 0f;
            set
            {
                if (!Is096)
                    return;
                Scp096.CurMaxShield = value;
            }
        }

        public float EnrageTimeLeft
        {
            get => Is096 ? Scp096.EnrageTimeLeft : 0f;
            set
            {
                if (!Is096)
                    return;
                Scp096.EnrageTimeLeft = value;
            }
        }

        public PlayableScps.Scp096PlayerState RageState
        {
            get => Is096 ? Scp096.PlayerState : PlayableScps.Scp096PlayerState.Docile;
            set
            {
                if (!Is096)
                    return;
                switch (value)
                {
                    case PlayableScps.Scp096PlayerState.Charging:
                        if (RageState != PlayableScps.Scp096PlayerState.Enraged)
                            RageState = PlayableScps.Scp096PlayerState.Enraged;
                        Scp096.Charge();
                        break;

                    case PlayableScps.Scp096PlayerState.Calming:
                        Scp096.EndEnrage();
                        break;

                    case PlayableScps.Scp096PlayerState.Enraged when RageState != PlayableScps.Scp096PlayerState.Attacking:
                        if (RageState == PlayableScps.Scp096PlayerState.Docile
                            || RageState == PlayableScps.Scp096PlayerState.TryNotToCry
                            || RageState == PlayableScps.Scp096PlayerState.Calming)
                        {
                            RageState = PlayableScps.Scp096PlayerState.Enraging;
                        }

                        Scp096.Enrage();
                        break;

                    case PlayableScps.Scp096PlayerState.Enraged when RageState == PlayableScps.Scp096PlayerState.Attacking:
                        Scp096.EndAttack();
                        break;

                    case PlayableScps.Scp096PlayerState.TryNotToCry:
                        if (RageState != PlayableScps.Scp096PlayerState.Docile)
                            RageState = PlayableScps.Scp096PlayerState.Docile;
                        Scp096.TryNotToCry();
                        break;

                    case PlayableScps.Scp096PlayerState.Attacking:
                        if (RageState != PlayableScps.Scp096PlayerState.Enraged)
                            RageState = PlayableScps.Scp096PlayerState.Enraged;
                        PlayableScps.Scp096.ServerDoAttack(player.Connection, default);
                        break;

                    case PlayableScps.Scp096PlayerState.Enraging:
                        if (RageState != PlayableScps.Scp096PlayerState.Docile)
                            RageState = PlayableScps.Scp096PlayerState.Docile;
                        Scp096.Windup();
                        break;

                    case PlayableScps.Scp096PlayerState.Docile:
                        Scp096.ResetEnrage();
                        break;

                        //Since you have to also enter a Door PryGate is not supported by this and you have to use ChargeDoor()
                }
            }
        }

        public List<Player> Targets => !Is096 ? new List<Player>() : Scp096._targets.Select(x => x.GetPlayer()).ToList();

        public bool CanAttack => Is096 && Scp096.CanAttack;

        public bool CanCharge => Is096 && Scp096.CanCharge;

        public void AddTarget(Player player)
        {
            if (!Is096 || !Scp096.CanReceiveTargets)
                return;

            Scp096.AddTarget(player.gameObject);
        }

        public void RemoveTarget(Player player)
        {
            if (!Is096)
                return;

            _ = Scp096._targets.Remove(player.Hub);
        }

        public void ChargeDoor(Door door)
        {
            if (!Is096)
                return;
            Scp096.ChargeDoor(door.VDoor);
        }
    }
}
