using System.Collections.Generic;
using CommandSystem.Commands.Shared;
using Neuron.Core;
using Neuron.Core.Meta;
using Neuron.Core.Modules;
using Neuron.Core.Plugins;
using Neuron.Modules.Commands;
using Neuron.Modules.Configs;
using Neuron.Modules.Patcher;
using Ninject;
using Synapse3.SynapseModule.Command;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Map.Schematic.CustomAttributes;
using Synapse3.SynapseModule.Map.Scp914;
using Synapse3.SynapseModule.Permissions.RemoteAdmin;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;
// ReSharper disable MemberCanBePrivate.Global

namespace Synapse3.SynapseModule;

[Module(
    Name = "Synapse",
    Description = "SCP:SL game functionality",
    Dependencies = new []
    {
        typeof(PatcherModule),
        typeof(CommandsModule),
        typeof(ConfigsModule)
    }
)]
public partial class Synapse : Module
{
    [Inject]
    public PatcherService Patcher { get; set; }
    
    [Inject]
    public CommandService Commands { get; set; }

    private IKernel _kernel;


    public SynapseCommandService SynapseCommandService { get; private set; }
    public RoleService RoleService { get; private set; }
    public TeamService TeamService { get; private set; }
    public CustomAttributeService CustomAttributeService { get; private set; }
    public Scp914Service Scp914Service { get; private set; }
    public ItemService ItemService { get; private set; }
    public RoomService RoomService { get; private set; }
    
    public RemoteAdminCategoryService RemoteAdminCategoryService { get; private set; }

    public override void Load(IKernel kernel)
    {
        Logger.Info("Synapse3 is loading");
        
        _kernel = kernel;
        var metaManager = kernel.Get<MetaManager>();
        var moduleManager = kernel.Get<ModuleManager>();
        var pluginManager = kernel.Get<PluginManager>();
        metaManager.MetaGenerateBindings.Subscribe(MetaGenerateBindings);
        moduleManager.ModuleLoadLate.Subscribe(LoadModuleLate);
        pluginManager.PluginLoadLate.Subscribe(OnPluginLoadLate);
        
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = $"Plugin Framework: Synapse\n" +
                                          $"Synapse Version: {GetVersion()}\n" +
                                          $"Description: Synapse is a heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";
        
        if(BasedGameVersion != GameCore.Version.VersionString)
            Logger.Warn($"Sy3 Version: This Version of Synapse3 is build for SCPSL Version {BasedGameVersion} Currently installed: {GameCore.Version.VersionString}\nBugs may occurs");
    }

    public override void Enable()
    {
        SynapseCommandService = _kernel.GetSafe<SynapseCommandService>();
        RoleService = _kernel.GetSafe<RoleService>();
        TeamService = _kernel.GetSafe<TeamService>();
        CustomAttributeService = _kernel.GetSafe<CustomAttributeService>();
        Scp914Service = _kernel.GetSafe<Scp914Service>();
        ItemService = _kernel.GetSafe<ItemService>();
        RoomService = _kernel.GetSafe<RoomService>();
        RemoteAdminCategoryService = _kernel.GetSafe<RemoteAdminCategoryService>();
        
        Logger.Info("Synapse3 enabled!");
    }
}