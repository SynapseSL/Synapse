using System.Collections.Generic;

namespace Synapse.Api.Roles
{
    public abstract class Role : IRole
    {
        private Player player;

        public Player Player
        {
            get => player;
            set
            {
                if (player == value)
                    return;

                if (player != null)
                    player.CustomRole = null;

                player = value;
            }
        }

        public abstract string GetRoleName();

        public abstract int GetRoleID();

        public abstract Team GetTeam();

        public virtual List<Team> GetFriends() => new List<Team>();

        public virtual List<Team> GetEnemys() => new List<Team>();

        public virtual int GetEscapeRole() => -1;

        public abstract void Spawn();

        public virtual void DeSpawn() { }

        public virtual void Escape() { }
    }
}
