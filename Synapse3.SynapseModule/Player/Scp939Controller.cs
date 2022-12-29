using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp939;

namespace Synapse3.SynapseModule.Player;

public class Scp939Controller : ScpShieldControler<Scp939Role>
{
    public Scp939Controller(SynapsePlayer player) : base(player) { }

    public override HumeShieldModuleBase SheildModule => Role?.HumeShieldModule;
}