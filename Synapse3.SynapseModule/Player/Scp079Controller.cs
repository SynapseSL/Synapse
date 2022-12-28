using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp173;
using Synapse3.SynapseModule.Map.Objects;

namespace Synapse3.SynapseModule.Player;

public class Scp079Controller
{
    //TODO:
    
    private readonly SynapsePlayer _player;
    
    internal Scp079Controller(SynapsePlayer player)
    {
        _player = player;
    }

    
    public Scp079Role Role => _player.CurrentRole as Scp079Role;
    public bool Is079Instance => Role != null;
    public Scp079TierManager TierManager => Role?.GetSubroutine<Scp079TierManager>();
    public Scp079AuxManager PowerManager => Role?.GetSubroutine<Scp079AuxManager>();
    public Scp079CurrentCameraSync CurrentCameraSync => Role?.GetSubroutine<Scp079CurrentCameraSync>();
    public Scp079DoorLockChanger DoorLockChanger => Role?.GetSubroutine<Scp079DoorLockChanger>();

    public int Level
    {
        get
        {
            if (TierManager != null) return TierManager.AccessTierIndex + 1;
            return 0;
        }
        set
        {
            if (TierManager == null) return;
            TierManager.AccessTierIndex = value - 1;
        }
    }

    public int Exp
    {
        get
        {
            if (TierManager != null) return TierManager.TotalExp;
            return 0;
        }
        set
        {
            if (TierManager == null) return;
            TierManager.TotalExp = value;
        }
    }

    public float Energy
    {
        get
        {
            if (PowerManager != null) return PowerManager.CurrentAux;
            return 0f;
        }
        set
        {
            if (PowerManager == null) return; 
            PowerManager.CurrentAux = value;
        }
    }

    private float _maxEnergy = -1;
    public float MaxEnergy
    {
        get
        {
            if (PowerManager != null)
            {
                if (_maxEnergy == -1)
                    return PowerManager._maxPerTier[PowerManager._tierManager.AccessTierIndex];
                return _maxEnergy;
            }
            return 0f;
        }
        set
        {
            if (PowerManager == null) return;
            _maxEnergy = value;
        }
    }

    public SynapseCamera Camera
    {
        get
        {
            if (CurrentCameraSync != null)
                return CurrentCameraSync.CurrentCamera.GetCamera();
            return null;
        }
        set
        {

            if (CurrentCameraSync == null) return;
            CurrentCameraSync.CurrentCamera = value.Camera;
        }
    }

    public void GiveExperience(int amount)
    {
        if (!Is079Instance) return;

        TierManager.ServerGrantExperience(amount, Scp079HudTranslation.ExpGainAdminCommand);
    }

    public void UnlockDoors()
    {
        if (!Is079Instance) return;

        DoorLockChanger.ServerUnlockAll();
    }
}