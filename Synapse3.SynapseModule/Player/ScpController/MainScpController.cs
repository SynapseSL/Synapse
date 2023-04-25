using PlayerRoles;
using Synapse3.SynapseModule.Config;

namespace Synapse3.SynapseModule.Player.ScpController;

public class MainScpController
{
    //TODO: Check Controllers
    private readonly SynapsePlayer _player;
    private readonly SynapseConfigService _config;

    internal MainScpController(SynapsePlayer player, SynapseConfigService config)
    {
        _player = player;
        Scp079 = new(player);
        Scp096 = new Scp096Controller(player);
        Scp106 = new Scp106Controller(player);
        Scp173 = new Scp173Controller(player);
        Scp939 = new Scp939Controller(player);
        _config = config;
    }

    public readonly Scp106Controller Scp106;

    public readonly Scp079Controller Scp079;

    public readonly Scp096Controller Scp096;

    public readonly Scp173Controller Scp173;

    public readonly Scp939Controller Scp939;

    public bool ProximityToggle(out string message, out bool enabled)
    {
        message = "";
        enabled = false;
        if (_player.Team != Team.SCPs) return false;
        if (!_config.GamePlayConfiguration.SpeakingScp.Contains(_player.RoleID) &&
            !_player.HasPermission("synapse.scp-proximity")) return false;
        
        enabled = ProximityChat = !ProximityChat;
        var translation = _config.Translation.Get(_player);
        message = _player.MainScpController.ProximityChat
            ? translation.EnableProximity
            : translation.DisableProximity;
        return true;
    }

    public bool ProximityChat { get; set; }

    public IScpControllerBase CurrentController =>
        _player.RoleType switch
        {
            RoleTypeId.Scp079 => Scp079,
            RoleTypeId.Scp096 => Scp096,
            RoleTypeId.Scp106 => Scp106,
            RoleTypeId.Scp173 => Scp173,
            RoleTypeId.Scp939 => Scp939,
            _ => null
        };
}