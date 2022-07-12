using Neuron.Core.Meta;
using Neuron.Modules.Configs;
using Synapse3.SynapseModule.Events;

namespace Synapse3.SynapseModule.Config;

public class SynapseConfigService : Service
{
    private ConfigService _configService;
    private readonly ServerEvents _server;

    public ConfigContainer Container { get; set; }

    public HostingConfiguration HostingConfiguration { get; private set; }
    
    public PermissionConfiguration PermissionConfiguration { get; private set; }
    
    public GamePlayConfiguration GamePlayConfiguration { get; internal set; }

    public SynapseConfigService(ConfigService configService, ServerEvents server)
    {
        _configService = configService;
        _server = server;
    }

    public override void Enable()
    {
        _server.Reload.Subscribe(Reload);
        Container = _configService.GetContainer("synapse.syml");
        Reload(null);
    }

    public override void Disable()
    {
        _server.Reload.Unsubscribe(Reload);
    }

    public void Reload(ReloadEvent _)
    {
        Container.Load();
        HostingConfiguration = Container.Get<HostingConfiguration>();
        PermissionConfiguration = Container.Get<PermissionConfiguration>();
        GamePlayConfiguration = Container.Get<GamePlayConfiguration>();
    }
}