using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Meta;
using Neuron.Modules.Commands.Event;
using Synapse3.SynapseModule.Command;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

public class RemoteAdminCategoryService : Service
{
    private readonly Synapse _synapseModule;
    private readonly SynapseCommandService _commandService;

    public RemoteAdminCategoryService(Synapse synapseModule, SynapseCommandService commandService)
    {
        _synapseModule = synapseModule;
        _commandService = commandService;
    }

    public override void Enable()
    {
        _commandService.RemoteAdmin.Subscribe(OnCommand);
        RegisterCategory<SynapseCategory>();
        RegisterCategory<OverWatchCategory>();
        RegisterCategory<InvisibleCategory>();
        RegisterCategory<GodModeCategory>();
        RegisterCategory<NoClipCategory>();

        while (_synapseModule.ModuleRaCategoryBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleRaCategoryBindingQueue.Dequeue();
            LoadBinding(binding);
        }
    }

    public override void Disable()
    {
        _commandService.RemoteAdmin.Unsubscribe(OnCommand);
    }

    internal void LoadBinding(SynapseRaCategoryBinding binding) => RegisterCategory(binding.Info);

    private readonly List<RemoteAdminCategory> _remoteAdminCategories = new();
    public ReadOnlyCollection<RemoteAdminCategory> RemoteAdminCategories => _remoteAdminCategories.AsReadOnly();

    public RemoteAdminCategory GetCategory(int id) => _remoteAdminCategories.FirstOrDefault(x => x.Attribute.Id == id);

    public void AddCategory<TCategory>(TCategory category) where TCategory : RemoteAdminCategory
    {
        if (!_remoteAdminCategories.Contains(category))
            _remoteAdminCategories.Add(category);
    }

    public void RegisterCategory<TCategory>() where TCategory : RemoteAdminCategory
    {
        var info = typeof(TCategory).GetCustomAttribute<RaCategoryAttribute>();
        if(info == null) return;
        info.CategoryType = typeof(TCategory);

        RegisterCategory(info);
    }

    public void RegisterCategory(RaCategoryAttribute info)
    {
        if (info.CategoryType == null) return;
        if (IsIdRegistered(info.Id)) return;

        var category = (RemoteAdminCategory)Synapse.GetOrCreate(info.CategoryType);
        category.Attribute = info;
        category.Load();
        
        _remoteAdminCategories.Add(category);
    }

    public bool IsIdRegistered(uint id) => _remoteAdminCategories.Any(x => x.Attribute.Id == id);

    private void OnCommand(CommandEvent ev)
    {
        if (ev.Context.Command.ToUpper() != "EXTERNALLOOKUP") return;
        if(ev.Context.Arguments.Length == 0) return;
        
        foreach (var category in _remoteAdminCategories)
        {
            if(ev.Context.Arguments[0] != category.Attribute.Id.ToString()) continue;
            ev.IsHandled = true;
            
            var context = (SynapseContext)ev.Context;
            context.Player.CommandSender.RaReply("% none % " + category.ExternalURL, true, false, "");
        }
    }
}