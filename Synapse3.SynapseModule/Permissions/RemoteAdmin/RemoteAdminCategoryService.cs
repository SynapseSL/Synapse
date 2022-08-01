using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Meta;
using Ninject;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

public class RemoteAdminCategoryService : Service
{
    private IKernel _kernel;
    private Synapse _synapseModule;

    public RemoteAdminCategoryService(IKernel kernel, Synapse synapseModule)
    {
        _kernel = kernel;
        _synapseModule = synapseModule;
    }

    public override void Enable()
    {
        RegisterCategory<SynapseCategory>();

        while (_synapseModule.ModuleRaCategoryBindingQueue.Count != 0)
        {
            var binding = _synapseModule.ModuleRaCategoryBindingQueue.Dequeue();
            LoadBinding(binding);
        }
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

        var category = (RemoteAdminCategory)_kernel.Get(info.CategoryType);
        _kernel.Bind(info.CategoryType).ToConstant(category).InSingletonScope();
        category.Attribute = info;
        _remoteAdminCategories.Add(category);
    }

    public bool IsIdRegistered(int id) => _remoteAdminCategories.Any(x => x.Attribute.Id == id);
}