using System.Collections.Generic;
using System.Linq;
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
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Map.Schematic.CustomAttributes;
using Synapse3.SynapseModule.Map.Scp914;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;
using Object = UnityEngine.Object;

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
public class Synapse : Module
{
    #region Static Utils
    public const int Major = 3;
    public const int Minor = 0;
    public const int Patch = 0;
    
    public const VersionType Type = VersionType.None;
    public const string SubVersion = "1.0";
    public const string BasedGameVersion = "11.2.1";
    
    /// <summary>
    /// Returns an instance of the specified object by either resolving it using
    /// ninject bindings (I.e. the object is already present in the ninject kernel)
    /// or by creating a new instance of the type using the ninject kernel making
    /// injection usable.
    /// </summary>
    public static T Get<T>() => Globals.Get<T>();
    
    /// <summary>
    /// Returns an List of all instances of the specified object from Unity
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static List<TObject> GetObjects<TObject>() where TObject : Object
    {
        return Object.FindObjectsOfType<TObject>().ToList();
    }

    /// <summary>
    /// Returns an instance of the specified object from Unity
    /// </summary>
    public static TObject GetObject<TObject>() where TObject : Object
    {
        return Object.FindObjectOfType<TObject>();
    }

    /// <summary>
    /// Creates a string with the full combined version of Synapse
    /// </summary>
    public static string GetVersion()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        
        if (Type != VersionType.None)
            version += $"{Type}-{SubVersion}";

        return version;
    }
    #endregion

    [Inject]
    public PatcherService Patcher { get; set; }
    
    [Inject]
    public CommandService Commands { get; set; }

    private IKernel _kernel;
    
    internal Queue<SynapseCommandBinding> moduleCommandBindingQueue = new();
    
    public SynapseCommandService SynapseCommandService { get; private set; }
    
    public RoleService RoleService { get; private set; }
    
    public TeamService TeamService { get; private set; }
    
    public CustomAttributeService CustomAttributeService { get; private set; }
    
    public Scp914Service Scp914Service { get; private set; }

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
                                          $"Synapse Version: {Synapse.GetVersion()}\n" +
                                          $"Description: Synapse is a heavily modded server software using extensive runtime patching to make development faster and the usage more accessible to end-users";
        
        if(Synapse.BasedGameVersion != GameCore.Version.VersionString)
            Logger.Warn($"Sy3 Version: This Version of Synapse3 is build for SCPSL Version {Synapse.BasedGameVersion} Currently installed: {GameCore.Version.VersionString}\nBugs may occurs");
    }

    private void MetaGenerateBindings(MetaGenerateBindingsEvent args)
    {
        OnGenerateRoleBinding(args);
        OnGenerateCommandBinding(args);
        OnGenerateTeamBinding(args);
        OnGenerateAttributeBinding(args);
        OnGenerate914ProcessorBinding(args);
    }

    private void OnGenerateRoleBinding(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out _)) return;
        if (!args.MetaType.TryGetAttribute<RoleAttribute>(out var roleInformation)) return;
        if (!args.MetaType.Is<ISynapseRole>()) return;

        roleInformation.RoleScript = args.MetaType.Type;
        args.Outputs.Add(new SynapseRoleBinding()
        {
            Info = roleInformation
        });
    }
    
    private void OnGenerateCommandBinding(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out _)) return;
        if (!args.MetaType.TryGetAttribute<SynapseCommandAttribute>(out var _)) return;
        if (!args.MetaType.Is<SynapseCommand>()) return;
        
        args.Outputs.Add(new SynapseCommandBinding()
        {
            Type = args.MetaType.Type,
        });
    }

    private void OnGenerateTeamBinding(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out _)) return;
        if (!args.MetaType.TryGetAttribute<TeamAttribute>(out var info)) return;
        if (!args.MetaType.Is<ISynapseTeam>()) return;
        
        args.Outputs.Add(new SynapseTeamBinding()
        {
            Info = info,
            Type = args.MetaType.Type
        });
    }

    private void OnGenerateAttributeBinding(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out var _)) return;
        if(!args.MetaType.Is<AttributeHandler>()) return;

        args.Outputs.Add(new SynapseCustomObjectAttributeBinding()
        {
            Type = args.MetaType.Type
        });
    }
    
    private void OnGenerate914ProcessorBinding(MetaGenerateBindingsEvent args)
    {
        if (!args.MetaType.TryGetAttribute<AutomaticAttribute>(out _)) return;
        if (!args.MetaType.TryGetAttribute<Scp914ProcessorAttribute>(out var info)) return;
        if(!args.MetaType.Is<ISynapse914Processor>()) return;

        args.Outputs.Add(new SynapseScp914ProcessorBinding()
        {
            Processor = args.MetaType.Type,
            ReplaceHandlers = info.ReplaceHandlers
        });
    }

    private void OnPluginLoadLate(PluginLoadEvent args)
    {
        args.Context.MetaBindings
            .OfType<SynapseCommandBinding>()
            .ToList().ForEach(x=> SynapseCommandService.LoadBinding(x));

        args.Context.MetaBindings
            .OfType<SynapseRoleBinding>()
            .ToList().ForEach(x => RoleService.LoadBinding(x));
        
        args.Context.MetaBindings
            .OfType<SynapseTeamBinding>()
            .ToList().ForEach(x => TeamService.LoadBinding(x));
        
        args.Context.MetaBindings
            .OfType<SynapseCustomObjectAttributeBinding>()
            .ToList().ForEach(x => CustomAttributeService.LoadBinding(x));
        
        args.Context.MetaBindings
            .OfType<SynapseScp914ProcessorBinding>()
            .ToList().ForEach(x => Scp914Service.LoadBinding(x));
    }

    private void LoadModuleLate(ModuleLoadEvent args)
    {
        args.Context.MetaBindings
            .OfType<SynapseCommandBinding>()
            .ToList().ForEach(binding =>
            {
                Logger.Debug("Enqueue module binding [SynapseCommand]");
                moduleCommandBindingQueue.Enqueue(binding);
            });
        
        args.Context.MetaBindings
            .OfType<SynapseRoleBinding>()
            .ToList().ForEach(x => RoleService.LoadBinding(x));
        
        args.Context.MetaBindings
            .OfType<SynapseTeamBinding>()
            .ToList().ForEach(x => TeamService.LoadBinding(x));
        
        args.Context.MetaBindings
            .OfType<SynapseCustomObjectAttributeBinding>()
            .ToList().ForEach(x => CustomAttributeService.LoadBinding(x));
        
        args.Context.MetaBindings
            .OfType<SynapseScp914ProcessorBinding>()
            .ToList().ForEach(x => Scp914Service.LoadBinding(x));
    }

    public override void Enable()
    {
        SynapseCommandService = _kernel.GetSafe<SynapseCommandService>();
        RoleService = _kernel.GetSafe<RoleService>();
        TeamService = _kernel.GetSafe<TeamService>();
        CustomAttributeService = _kernel.GetSafe<CustomAttributeService>();
        Scp914Service = _kernel.GetSafe<Scp914Service>();
        
        Logger.Info("Synapse3 enabled!");
    }

    public override void Disable()
    {
        
    }
}