using Synapse3.SynapseModule.Map.Objects;

namespace Synapse3.SynapseModule.Player;

public class Scp079Controller
{
    private readonly SynapsePlayer _player;
    
    internal Scp079Controller(SynapsePlayer player)
    {
        _player = player;
    }

    public Scp079PlayerScript Scp079Script => _player.ClassManager.Scp079;
    
    public byte Level
    {
        get => (byte)(Scp079Script.Lvl + 1);
        set => Scp079Script.Lvl = (byte)(value - 1);
    }

    public float Exp
    {
        get => Scp079Script.Exp;
        set => Scp079Script.Exp = value;
    }

    public float Energy
    {
        get => Scp079Script.Mana;
        set => Scp079Script.Mana = value;
    }

    public float MaxEnergy
    {
        get => Scp079Script.maxMana;
        set => Scp079Script.NetworkmaxMana = value;
    }

    public SynapseCamera Camera
    {
        get => Scp079Script.currentCamera.GetCamera();
        set => Scp079Script.RpcSwitchCamera(value.CameraID, false);
    }

    public void GiveExperience(float amount) => Scp079Script.AddExperience(amount);

    public void ForceLevel(byte level) => Scp079Script.ForceLevel(level, true);

    public void UnlockDoors() => Scp079Script.CmdResetDoors();
}