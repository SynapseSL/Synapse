using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;

namespace Synapse3.SynapseModule.Player.ScpController;

public interface IScpControllerBase
{
    public RoleTypeId ScpRole { get; }
    public bool IsInstance { get; }
}

public abstract class ScpControllerBase<T> : IScpControllerBase
    where T : PlayerRoleBase
{
    protected readonly SynapsePlayer _player;
    internal ScpControllerBase(SynapsePlayer player) =>  _player = player;

    public T Role => _player.CurrentRole as T;
    public bool IsInstance => _player.CurrentRole is T;

    public abstract RoleTypeId ScpRole { get; }
}

public interface IScpShieldController
{
    public float CurrentShield { get; set; }

    public float MaxShield { get; set; }
    public  bool UseDefaultMaxShield { get; }

    public float ShieldRegeneration { get; set; }
    public  bool UseDefaultShieldRegeneration { get; }
}

public abstract class ScpShieldController<T> : ScpControllerBase<T>, IScpShieldController
    where T : PlayerRoleBase
{
    public HumeShieldModuleBase ShieldModule => (_player.CurrentRole as IHumeShieldedRole)?.HumeShieldModule;

    internal ScpShieldController(SynapsePlayer player) : base(player) { }

    public float CurrentShield
    {
        get
        {
            if (IsInstance) return ShieldModule.HsCurrent;
            return 0;
        }
        set
        {
            if (!IsInstance) return;
            ShieldModule.HsCurrent = value;
        }
    }

    internal float _maxShield = -1;
    /// <summary>
    /// The max shiled of the scp (set to -1 to use the default one)
    /// </summary>
    public float MaxShield
    {
        get
        {
            if (IsInstance)
            {
                return UseDefaultMaxShield ? ShieldModule.HsMax : _maxShield;
            }
            return 0f;
        }
        set
        {
            if (!IsInstance) return;
            _maxShield = value;
        }
    }
    public bool UseDefaultMaxShield => _maxShield < 0;
    
    
    
    internal float _shieldRegeneration = -1;
    /// <summary>
    /// The regeneration of the shiled (set to -1 to use the default one)
    /// </summary>
    public float ShieldRegeneration
    {
        get
        {
            if (IsInstance)
            {
                return UseDefaultShieldRegeneration ? ShieldModule.HsRegeneration : _shieldRegeneration;
            }
            return 0f;
        }
        set
        {
            if (!IsInstance) return;
            _shieldRegeneration = value;
        }
    }
    public bool UseDefaultShieldRegeneration => _shieldRegeneration < 0;
}
