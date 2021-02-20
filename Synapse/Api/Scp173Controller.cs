using System.Collections.Generic;

namespace Synapse.Api
{
    public class Scp173Controller
    {
        internal Scp173Controller(Player _player) => player = _player;

        private Player player;

        public HashSet<Player> IgnoredPlayers { get; internal set; } = new HashSet<Player>();

        public HashSet<Player> TurnedPlayers { get; internal set; } = new HashSet<Player>();

        public HashSet<Player> ConfrontingPlayers { get; internal set; } = new HashSet<Player>();
    }
}
