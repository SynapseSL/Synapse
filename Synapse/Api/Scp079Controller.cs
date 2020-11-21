namespace Synapse.Api
{
    public class Scp079Controller
    {
        internal Scp079Controller(Player player) => this.player = player;

        private readonly Player player;

        private Scp079PlayerScript script => player.ClassManager.Scp079;


        public bool Is079 => player.RoleType == RoleType.Scp079;

        public byte Level { get => script.Lvl; set => script.Lvl = value; }

        public string Speaker { get => script.Speaker; set => script.Speaker = value; }

        public float Exp { get => script.Exp; set => script.Exp = value; }

        public float Energy { get => script.Mana; set => script.Mana = value; }

        public float MaxEnergy { get => script.maxMana; set => script.NetworkmaxMana = value; }

        /// <summary>
        /// The current camera the player uses (Scp079 only, if not null)
        /// </summary>
        public Camera079 Camera { get => script.currentCamera; set => script?.RpcSwitchCamera(value.cameraId, false); }


        public void GiveExperience(float amount) => script.AddExperience(amount);

        public void ForceLevel(byte levelToForce, bool notifiyUser) => script.ForceLevel(levelToForce, notifiyUser);

        public void UnlockDoors() => script.CmdResetDoors();
    }
}
