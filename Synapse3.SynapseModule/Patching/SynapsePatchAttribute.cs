using HarmonyLib;

namespace Synapse3.SynapseModule.Patching;

public class SynapsePatchAttribute : HarmonyPatch
{
    public SynapsePatchAttribute(string name, PatchType patchType)
    {
        Name = name;
        PatchType = patchType;
    }
    
    public string Name { get; }
    
    public PatchType PatchType { get; }
}