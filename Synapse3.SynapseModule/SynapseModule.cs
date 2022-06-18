using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem.Commands.Shared;
using Neuron.Core;
using Neuron.Core.Meta;
using Neuron.Core.Modules;
using Neuron.Core.Plugins;
using Neuron.Modules.Patcher;
using Neuron.Modules.Commands;
using Neuron.Modules.Configs;
using Ninject;
using Syml;
using Synapse3.SynapseModule.Command;

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
public class SynapseModule : Module
{
    [Inject]
    public PatcherService Patcher { get; set; }
    
    [Inject]
    public CommandService Commands { get; set; }

    private IKernel _kernel;
    
    internal Queue<SynapseCommandBinding> moduleCommandBindingQueue = new();
    
    public SynapseCommandService SynapseCommandService { get; set; }

    public override void Load(IKernel kernel)
    {
        Logger.Info("Synapse3 is loading");
        
        _kernel = kernel;
        var metaManager = kernel.Get<MetaManager>();
        var moduleManager = kernel.Get<ModuleManager>();
        var pluginManager = kernel.Get<PluginManager>();
        metaManager.MetaGenerateBindings.Subscribe(OnGenerateCommandBinding);
        moduleManager.ModuleLoadLate.Subscribe(OnModuleLoadCommands);
        pluginManager.PluginLoadLate.Subscribe(OnPluginLoadCommands);
        
        CustomNetworkManager.Modded = true;
        BuildInfoCommand.ModDescription = $"Plugin Framework: Synapse\n" +
                                          $"Synapse Version: {Synapse.GetVersion()}\n" +
                                          $"Description: Synapse is a heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";
        
        if(Synapse.BasedGameVersion != GameCore.Version.VersionString)
            Logger.Warn($"Sy3 Version: This Version of Synapse3 is build for SCPSL Version {Synapse.BasedGameVersion} Currently installed: {GameCore.Version.VersionString}\nBugs may occurs");
    }
    
    private void OnGenerateCommandBinding(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out var automaticAttribute)) return;
        if (!args.MetaType.TryGetAttribute<SynapseCommandAttribute>(out var commandAttribute)) return;
        if (!args.MetaType.Is<SynapseCommand>()) return;
            
        Logger.Debug($"* {args.MetaType.Type} [SynapseCommandBinding]");
        args.Outputs.Add(new SynapseCommandBinding()
        {
            Type = args.MetaType.Type,
        });
    }

    private void OnPluginLoadCommands(PluginLoadEvent args) => args.Context.MetaBindings
        .OfType<SynapseCommandBinding>()
        .ToList().ForEach(x=> SynapseCommandService.LoadBinding(x));

    private void OnModuleLoadCommands(ModuleLoadEvent args) => args.Context.MetaBindings
        .OfType<SynapseCommandBinding>()
        .ToList().ForEach(binding =>
        {
            Logger.Debug("Enqueue module binding [SynapseCommand]");
            moduleCommandBindingQueue.Enqueue(binding);
        });

    public override void Enable()
    {
        SynapseCommandService = _kernel.GetSafe<SynapseCommandService>();
        
        Logger.Info("Synapse3 enabled!");
    }

    public override void Disable()
    {
        
    }
}

public class SynapseCommandBinding : IMetaBinding
{
    
    public Type Type { get; set; }

    public IEnumerable<Type> PromisedServices => new Type[] { };
}