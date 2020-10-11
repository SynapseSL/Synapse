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

        public List<Items.Item> Items { get; set; } = new List<Items.Item>();

        public float Health { get; set; }

        public void JailPlayer(Player admin)
        {
            if (IsJailed) return;

            Admin = admin;
            Role = Player.RoleType;
            Position = Player.Position;

            Items.Clear();
            foreach (var item in Player.Inventory.Items)
            {
                Items.Add(item);
                item.Despawn();
            }

            Health = Player.Health;

            Player.RoleType = RoleType.Tutorial;

            IsJailed = true;
        }

        public void UnJailPlayer()
        {
            if (!IsJailed) return;

            Player.ChangeRoleAtPosition(Role);
            Player.Position = Position;
            Player.Health = Health;
            Player.Inventory.Clear();

            foreach (var item in Items)
                Player.Inventory.AddItem(item);

            IsJailed = false;
        }
    }
}
