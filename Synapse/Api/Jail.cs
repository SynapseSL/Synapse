using Synapse.Config;

namespace Synapse.Api
{
    public class Jail
    {
        internal Jail(Player player)
            => _player = player;

        private readonly Player _player;
        private bool isjailed;

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

        public Player Admin { get; private set; }

        public SerializedPlayerState State { get; private set; }

        public void JailPlayer(Player admin)
        {
            if (IsJailed)
                return;

            Admin = admin;
            State = _player;

            new SerializedPlayerState()
            {
                Position = Admin.Position,
                RoleType = RoleType.Tutorial,
            }.Apply(_player);

            isjailed = true;
        }

        public void UnJailPlayer()
        {
            if (!IsJailed)
                return;

            State.Apply(_player, true);

            isjailed = false;
        }
    }
}
