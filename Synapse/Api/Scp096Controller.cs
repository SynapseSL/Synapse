using PlayableScps;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api
{
    public class Scp096Controller
    {
        internal Scp096Controller(Player _player) => player = _player;

        private readonly Player player;

        private Scp096 Scp096 => player.Hub.scpsController.CurrentScp as Scp096;

        public bool Is096 => player.RoleType is RoleType.Scp096;

        public float ShieldAmount
        {
            get
            {
                if (Is096) return Scp096.ShieldAmount;
                return 0;
            }
            set
            {
                if (!Is096) return;
                Scp096.ShieldAmount = value;
            }
        }

        public float MaxShield { get; set; } = 350f;

        public float CurMaxShield
        {
            get
            {
                if (Is096) return Scp096.CurMaxShield;
                return 0f;
            }
            set
            {
                if (!Is096) return;
                Scp096.CurMaxShield = value;
            }
        }

        public float EnrageTimeLeft
        {
            get
            {
                if (Is096) return Scp096.EnrageTimeLeft;
                return 0f;
            }
            set
            {
                if (!Is096) return;
                Scp096.EnrageTimeLeft = value;
            }
        }

        public PlayableScps.Scp096PlayerState RageState
        {
            get
            {
                if (Is096) return Scp096.PlayerState;
                return PlayableScps.Scp096PlayerState.Docile;
            }
            set
            {
                if (!Is096) return;
                switch (value)
                {
                    case Scp096PlayerState.Charging:
                        if (RageState is not Scp096PlayerState.Enraged)
                            RageState = Scp096PlayerState.Enraged;
                        Scp096.Charge();
                        break;

                    case Scp096PlayerState.Calming:
                        Scp096.EndEnrage();
                        break;

                    case Scp096PlayerState.Enraged when RageState is not Scp096PlayerState.Attacking:
                        if (RageState is Scp096PlayerState.Docile or Scp096PlayerState.TryNotToCry or Scp096PlayerState.Calming)
                            RageState = Scp096PlayerState.Enraging;
                        Scp096.Enrage();
                        break;

                    case Scp096PlayerState.Enraged when RageState is Scp096PlayerState.Attacking:
                        Scp096.EndAttack();
                        break;

                    case Scp096PlayerState.TryNotToCry:
                        if (RageState is not Scp096PlayerState.Docile)
                            RageState = Scp096PlayerState.Docile;
                        Scp096.TryNotToCry();
                        break;

                    case PlayableScps.Scp096PlayerState.Attacking:
                        if (RageState is not Scp096PlayerState.Enraged)
                            RageState = Scp096PlayerState.Enraged;
                        PlayableScps.Scp096.ServerDoAttack(player.Connection, default);
                        break;

                    case Scp096PlayerState.Enraging:
                        if (RageState is not Scp096PlayerState.Docile)
                            RageState = Scp096PlayerState.Docile;
                        Scp096.Windup();
                        break;

                    case Scp096PlayerState.Docile:
                        Scp096.ResetEnrage();
                        break;

                        //Since you have to also enter a Door PryGate is not supported by this and you have to use ChargeDoor()
                }
            }
        }

        public List<Player> Targets
        {
            get
            {
                if (!Is096) return new();
                return Scp096._targets.Select(x => x.GetPlayer()).ToList();
            }
        }

        public bool CanAttack
        {
            get
            {
                if (Is096) return Scp096.CanAttack;
                return false;
            }
        }

        public bool CanCharge
        {
            get
            {
                if (Is096) return Scp096.CanCharge;
                return false;
            }
        }

        public void AddTarget(Player player)
        {
            if (!Is096 || !Scp096.CanReceiveTargets) return;

            Scp096.AddTarget(player.gameObject);
        }

        public void RemoveTarget(Player player)
        {
            if (!Is096) return;

            Scp096._targets.Remove(player.Hub);
        }

        public void ChargeDoor(Door door)
        {
            if (!Is096) return;
            Scp096.ChargeDoor(door.VDoor);
        }
    }
}