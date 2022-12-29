using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp173;
using Synapse3.SynapseModule.Map.Objects;

namespace Synapse3.SynapseModule.Player;


public class Scp079Controller : ScpControllerBase<Scp079Role>
{
    //TODO:
    
    private readonly SynapsePlayer _player;
    
    internal Scp079Controller(SynapsePlayer player) : base(player) { }


    public Scp079TierManager TierManager => Role?.GetSubroutine<Scp079TierManager>();
    public Scp079AuxManager PowerManager => Role?.GetSubroutine<Scp079AuxManager>();
    public Scp079CurrentCameraSync CurrentCameraSync => Role?.GetSubroutine<Scp079CurrentCameraSync>();
    public Scp079DoorLockChanger DoorLockChanger => Role?.GetSubroutine<Scp079DoorLockChanger>();

    public int Level//TODO: Fix It
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
            TierManager.ServerSendRpc(toAll: true);
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
            TierManager.ServerSendRpc(toAll: true);
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
            TierManager.ServerSendRpc(toAll: true);
        }
    }

    private float _maxEnergy = -1;
    public float MaxEnergy//Work but not display for the player
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

    private float _regenEnergy = -1;
    public float RegenEnergy
    {
        get
        {
            if (PowerManager != null)
            {
                if (_regenEnergy == -1)
                {
                    float num = PowerManager._regenerationPerTier[PowerManager._tierManager.AccessTierIndex];
                    for (int i = 0; i < PowerManager._abilitiesCount; i++)
                    {
                        num *= PowerManager._abilities[i].AuxRegenMultiplier;
                    }
                    return num;
                }
                return _regenEnergy;
            }
            return 0f;
        }
        set
        {
            if (PowerManager == null) return;
            _regenEnergy = value;
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

    public void GiveExperience(int amount, Scp079HudTranslation reason = Scp079HudTranslation.ExpGainAdminCommand)
    {
        if (!IsInstance) return;

        TierManager.ServerGrantExperience(amount, reason);
    }

    public void UnlockDoors()
    {
        if (!IsInstance) return;

        DoorLockChanger.ServerUnlockAll();
    }
}