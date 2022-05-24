using Synapse.Api.Enum;
using System.Collections.Generic;
using Synapse.Config;
using UnityEngine;

namespace Synapse.Api
{
    public class Jail
    {
        internal Jail(Player player) => Player = player;

        private readonly Player Player;
        private bool isjailed = false;

        public bool IsJailed
        {
            get => isjailed;
            set
            {
                if (value)
                    JailPlayer(Server.Get.Host);
                else
                    UnJailPlayer();
            }
        }

        public Player Admin { get; set; }

        public  SerializedPlayerState State { get; set; }

        public void JailPlayer(Player admin)
        {
            if (IsJailed) return;

            Admin = admin;
            State = Player;
            
            new SerializedPlayerState()
            {
                Position = Admin.Position,
                RoleType = RoleType.Tutorial,
            }.Apply(Player);

            isjailed = true;
        }

        public void UnJailPlayer()
        {
            if (!IsJailed) return;

            State.Apply(Player, true);

            isjailed = false;
        }
    }
}
