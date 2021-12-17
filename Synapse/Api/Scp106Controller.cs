using System.Collections.Generic;
using UnityEngine;

namespace Synapse.Api
{
    public class Scp106Controller
    {
        internal Scp106Controller(Player player) => this.player = player;

        private readonly Player player;

        private Scp106PlayerScript Script => player.ClassManager.Scp106;

        public bool Is106 => player.RoleType == RoleType.Scp106;

        public Vector3 PortalPosition { get => Script.NetworkportalPosition; set => Script.SetPortalPosition(Vector3.zero, value); }

        public bool IsUsingPortal => Script.goingViaThePortal;

        public HashSet<Player> PocketPlayers { get; } = new HashSet<Player>();


        public void UsePortal() => Script.UserCode_CmdUsePortal();

        public void DeletePortal() => Script.DeletePortal();

        public void CreatePortal() => Script.CreatePortalInCurrentPosition();

        public void Contain() => Script.Contain(new Footprinting.Footprint(player.Hub));

        public void CapturePlayer(Player player) => Script.UserCode_CmdMovePlayer(player.gameObject, ServerTime.time);
    }
}
