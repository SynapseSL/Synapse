using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Service that manages the Synapse configuration file
/// </summary>
public class SynapseConfigService : Service
{
    private ConfigService _configService;
    private readonly ServerEvents _server;
    private readonly PlayerEvents _player;

    public ConfigContainer Container { get; set; }

    public HostingConfiguration HostingConfiguration { get; private set; }
    
    public JoinMessageConfiguration JoinMessageConfiguration { get; private set; }
    
    public PermissionConfiguration PermissionConfiguration { get; private set; }
    
    public GamePlayConfiguration GamePlayConfiguration { get; private set; }

    public SynapseConfigService(ConfigService configService, ServerEvents server, PlayerEvents player)
    {
        _configService = configService;
        _server = server;
        _player = player;
    }

    public override void Enable()
    {
        _server.Reload.Subscribe(Reload);
        _player.Join.Subscribe(OnJoin);
        Container = _configService.GetContainer("synapse.syml");
        Reload();
    }

    public override void Disable()
    {
        _server.Reload.Unsubscribe(Reload);
        _player.Join.Unsubscribe(OnJoin);
    }

    public void Reload(ReloadEvent _ = null)
    {
        Container.Load();
        HostingConfiguration = Container.Get<HostingConfiguration>();
        JoinMessageConfiguration = Container.Get<JoinMessageConfiguration>();
        PermissionConfiguration = Container.Get<PermissionConfiguration>();
        GamePlayConfiguration = Container.Get<GamePlayConfiguration>();
    }

    private void OnJoin(JoinEvent ev)
    {
        if (!string.IsNullOrWhiteSpace(JoinMessageConfiguration.Broadcast) &&
            JoinMessageConfiguration.BroadcastDuration > 0)
            ev.Player.SendBroadcast(JoinMessageConfiguration.Broadcast, JoinMessageConfiguration.BroadcastDuration);

        if (!string.IsNullOrWhiteSpace(JoinMessageConfiguration.Hint) &&
            JoinMessageConfiguration.HintDuration > 0)
            ev.Player.SendHint(JoinMessageConfiguration.Hint, JoinMessageConfiguration.HintDuration);

        if (!string.IsNullOrWhiteSpace(JoinMessageConfiguration.Window))
            ev.Player.SendWindowMessage(JoinMessageConfiguration.Window);
    }
}