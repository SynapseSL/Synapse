namespace Synapse.Api
{
    public class Scp079Controller
    {
        internal Scp079Controller(Player player)
            => _player = player;

        private readonly Player _player;

        private Scp079PlayerScript Script
            => _player.ClassManager.Scp079;

        internal bool Spawned { get; set; }

        public bool Is079
            => _player.RoleType == RoleType.Scp079;

        public byte Level
        {
            get => (byte)(Script.Lvl + 1);
            set => Script.Lvl = (byte)(value - 1);
        }

        public string Speaker
        {
            get => Script.Speaker;
            set => Script.Speaker = value;
        }

        public float Exp
        {
            get => Script.Exp;
            set => Script.Exp = value;
        }

        public float Energy
        {
            get => Script.Mana;
            set => Script.Mana = value;
        }

        public float MaxEnergy
        {
            get => Script.maxMana;
            set => Script.NetworkmaxMana = value;
        }

        /// <summary>
        /// The current camera the player uses (Scp079 only, if not null)
        /// </summary>
        public Camera Camera
        {
            get => Script.currentCamera.GetSynapseCamera();
            set => Script?.RpcSwitchCamera(value.ID, false);
        }

        public void GiveExperience(float amount)
            => Script.AddExperience(amount);

        public void ForceLevel(byte levelToForce, bool notifiyUser)
            => Script.ForceLevel(levelToForce, notifiyUser);

        public void UnlockDoors()
            => Script.CmdResetDoors();
    }
}
