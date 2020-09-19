using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Synapse.Api
{
    public class Jail
    {
        internal Jail(Player player) => Player = player;

        private Player Player;
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

        public RoleType Role { get; set; }

        public Vector3 Position { get; set; }

        public List<Inventory.SyncItemInfo> Items { get; set; } = new List<Inventory.SyncItemInfo>();

        public float Health { get; set; }

        public void JailPlayer(Player admin)
        {
            if (IsJailed) return;

            Admin = admin;
            Role = Player.Role;
            Position = Player.Position;

            Items.Clear();
            foreach (var item in Player.Items)
                Items.Add(item);

            Health = Player.Health;

            Player.Role = RoleType.Tutorial;

            IsJailed = true;
        }

        public void UnJailPlayer()
        {
            if (!IsJailed) return;

            Player.ChangeRoleAtPosition(Role);
            Player.Position = Position;
            Player.Health = Health;
            Player.ClearInventory();

            foreach (var item in Items)
                Player.Inventory.items.Add(item);

            IsJailed = false;

        }
    }
}
