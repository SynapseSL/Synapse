using System.Collections.Generic;
using System.Linq;
using System;

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

#pragma warning disable CS0618
        public virtual int GetTeamID() => (int)GetTeam();

        public virtual List<int> GetFriendsID() => GetFriends().Select(x => (int)x).ToList();

        public virtual List<int> GetEnemiesID() => GetEnemys().Select(x => (int)x).ToList();
#pragma warning restore CS0618

        public abstract void Spawn();

        public virtual void DeSpawn() { }

        public virtual void Escape() { }
        // why?
        #region Obsolete
        [Obsolete("CustomRoles will now be ignored when Escape and the Escape method is called so use it instead")]
        public virtual int GetEscapeRole() => -1;
        [Obsolete("Use GetTeamID()", false)]
        public virtual Team GetTeam() => Team.RIP;
        [Obsolete("Use GetFriendsID()", false)]
        public virtual List<Team> GetFriends() => new List<Team>();
        [Obsolete("Use GetEnemiesID()", false)]
        public virtual List<Team> GetEnemys() => new List<Team>();
        #endregion
    }
}