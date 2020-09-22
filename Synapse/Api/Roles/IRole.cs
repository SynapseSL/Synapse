using System.Collections.Generic;

namespace Synapse.Api.Roles
{
    public interface IRole
    {
        Player Player { get; set; }

        string GetRoleName();

        int GetRoleID();

        Team GetTeam();

        List<Team> GetFriends();

        List<Team> GetEnemys();

        RoleType GetEscapeRole();

        void Escape();

        void Spawn();

        void DeSpawn();
    }
}
