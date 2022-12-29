using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp106;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse3.SynapseModule.Player;

public abstract class ScpControllerBase<T> where T : PlayerRoleBase
{
    private readonly SynapsePlayer _player;

    internal ScpControllerBase(SynapsePlayer player)
    {
        _player = player;
    }

    public T Role => _player.CurrentRole as T;
    public bool IsInstance => Role != null;

}

public abstract class ScpShieldControler<T> : ScpControllerBase<T> where T : PlayerRoleBase
{
    public abstract HumeShieldModuleBase SheildModule { get; }

    internal ScpShieldControler(SynapsePlayer player) : base(player) { }

    public float CurentShield
    {
        get
        {
            if (IsInstance) return SheildModule.HsCurrent;
            return 0;
        }
        set
        {
            if (!IsInstance) return;
            SheildModule.HsCurrent = value;
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
                if (_maxShield == -1)
                    return SheildModule.HsMax;
                else
                    return _maxShield;
            }
            return 0f;
        }
        set
        {
            _maxShield = value;
        }
    }

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
                if (_shieldRegeneration != -1) return _shieldRegeneration;
                
                return SheildModule.HsRegeneration;
            }
            return 0f;
        }
        set
        {
            if (!IsInstance) return;
            _shieldRegeneration = value;
        }
    }
}
