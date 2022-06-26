using Neuron.Core.Meta;
using Neuron.Modules.Configs;

namespace Synapse3.SynapseModule.Config;

public class SynapseConfigService : Service
{
    private ConfigService _configService;
    public ConfigContainer Container { get; set; }

    public HostingConfiguration HostingConfiguration { get; private set; }
    
    public PermissionConfiguration PermissionConfiguration { get; private set; }

    public SynapseConfigService(ConfigService configService)
    {
        _configService = configService;
    }

    public override void Enable()
    {
        Container = _configService.GetContainer("synapse.syml");
        Reload();
    }

    public void Reload()
    {
        HostingConfiguration = Container.Get<HostingConfiguration>();
        PermissionConfiguration = Container.Get<PermissionConfiguration>();
    }
}