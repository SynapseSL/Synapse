using System.Collections.Generic;

namespace Synapse.Api.Roles
{
    public abstract class Role : IRole
    {
        public Player Player { get; set; }

        public abstract string GetRoleName();

        public abstract Team GetTeam();

        public virtual List<Team> GetFriends() => new List<Team>();

        public virtual List<Team> GetEnemys() => new List<Team>();

        public virtual RoleType GetEscapeRole() => RoleType.None;

        public abstract void Spawn();

        public virtual void DeSpawn()
        {
            Player.CustomRole = null;
        }

        public virtual void Escape() { }
    }
}
