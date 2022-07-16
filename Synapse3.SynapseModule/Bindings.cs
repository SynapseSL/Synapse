using System;
using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Role;
using Synapse3.SynapseModule.Teams;

namespace Synapse3.SynapseModule;

public class SynapseCommandBinding : IMetaBinding
{
    public Type Type { get; set; }

    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseRoleBinding : IMetaBinding
{
    public RoleAttribute Info { get; set; }

    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseTeamBinding : IMetaBinding
{
    public TeamAttribute Info { get; set; }
    
    public Type Type { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseCustomObjectAttributeBinding : IMetaBinding
{
    public Type Type { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseScp914ProcessorBinding : IMetaBinding
{
    public Type Processor { get; set; }
    
    public int[] ReplaceHandlers { get; set; }

    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseItemBinding : IMetaBinding
{
    public ItemAttribute Info { get; set; }
    
    public Type HandlerType { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}