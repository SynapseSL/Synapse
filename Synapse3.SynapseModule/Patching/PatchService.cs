using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Neuron.Core.Meta;
using Neuron.Modules.Patcher;

namespace Synapse3.SynapseModule.Patching;

public class PatchService : Service
{
    private readonly PatcherService _patcherService;

    public PatchService(PatcherService patcherService) => _patcherService = patcherService;
    
    public ReadOnlyDictionary<SynapsePatchAttribute, Type> Patches { get; private set; }

    public override void Enable()
    {
        var patches = new Dictionary<SynapsePatchAttribute, Type>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var attribute = type.GetCustomAttribute<SynapsePatchAttribute>();
            if (attribute == null) continue;
            patches[attribute] = type;
        }

        Patches = new ReadOnlyDictionary<SynapsePatchAttribute, Type>(patches);
    }

    public void DisablePatches(PatchType patchType)
    {
        foreach (var patch in Patches)
        {
            if(patch.Key.PatchType != patchType) continue;
            _patcherService.UnPatchType(patch.Value);
        }
    }
    
    public void DisablePatch(string name)
    {
        foreach (var patch in Patches)
        {
            if (!string.Equals(patch.Key.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
            _patcherService.UnPatchType(patch.Value);
        }
    }

    public void DisablePatch(Type patchType) => _patcherService.UnPatchType(patchType);
}