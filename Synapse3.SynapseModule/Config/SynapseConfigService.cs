using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using Neuron.Modules.Configs.Localization;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Config;

/// <summary>
/// The Synapse Service that manages the Synapse configuration file
/// </summary>
public class SynapseConfigService : Service
{
    private readonly ConfigService _config;
    private readonly TranslationService _translation;
    private readonly ServerEvents _server;
    private readonly PlayerEvents _player;

    public ConfigContainer Container { get; set; }

    public SynapseTranslation Translation { get; set; }
    
    public HostingConfiguration HostingConfiguration { get; private set; }

    public PermissionConfiguration PermissionConfiguration { get; private set; }
    
    public GamePlayConfiguration GamePlayConfiguration { get; private set; }

    public SynapseConfigService(ConfigService config, TranslationService translation, ServerEvents server,
        PlayerEvents player)
    {
        _config = config;
        _translation = translation;
        _server = server;
        _player = player;
    }

    public override void Enable()
    {
        _server.Reload.Subscribe(Reload);
        _player.Join.Subscribe(OnJoin);
        Container = _config.GetContainer("synapse.syml");
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
        PermissionConfiguration = Container.Get<PermissionConfiguration>();
        GamePlayConfiguration = Container.Get<GamePlayConfiguration>();
        
        _config.ReloadModuleConfigs();
        _config.ReloadPluginConfigs();
        //TODO: _translation.Reload();
        Translation = Synapse.Get<SynapseTranslation>();
    }

    private void OnJoin(JoinEvent ev)
    {
        var playerTranslation = ev.Player.GetTranslation(Translation);
        if (!string.IsNullOrWhiteSpace(playerTranslation.Broadcast) &&
            playerTranslation.BroadcastDuration > 0)
            ev.Player.SendBroadcast(playerTranslation.Broadcast, playerTranslation.BroadcastDuration);

        if (!string.IsNullOrWhiteSpace(playerTranslation.Hint) &&
            playerTranslation.HintDuration > 0)
            ev.Player.SendHint(playerTranslation.Hint, playerTranslation.HintDuration);

        if (!string.IsNullOrWhiteSpace(playerTranslation.Window))
            ev.Player.SendWindowMessage(playerTranslation.Window);
    }
}