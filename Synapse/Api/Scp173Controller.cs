using PlayableScps;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Api
{
    public class Scp173Controller
    {
        private readonly Player _player;

        internal Scp173Controller(Player player) 
            => _player = player;
       
        private Scp173 Scp173
            => _player.Hub.scpsController.CurrentScp as Scp173;

        public bool Is173
            => _player.RoleType == RoleType.Scp173;

        public bool IsObserved => Scp173._isObserved;

        public float TimeBlinck { get => Scp173._blinkCooldownRemaining; set => Scp173._blinkCooldownRemaining = value; }

        public HashSet<Player> ConfrontingPlayers { get; internal set; } = new HashSet<Player>();

        public HashSet<Player> IgnoredPlayers { get; internal set; } = new HashSet<Player>();

        public HashSet<Player> TurnedPlayers { get; internal set; } = new HashSet<Player>();

    }
}
