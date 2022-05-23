using System.Collections.Generic;
namespace Synapse.Api.Roles
{
    public interface IRole
    {
        Player Player { get; set; }

        string GetRoleName();

        int GetRoleID();

        int GetTeamID();

        List<int> GetFriendsID();

        List<int> GetEnemiesID();

        void Escape();

        void Spawn();

        void DeSpawn();
    }
}