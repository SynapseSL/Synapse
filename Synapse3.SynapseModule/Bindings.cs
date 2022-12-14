using System;
using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Database;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Map.Rooms;
using Synapse3.SynapseModule.Permissions.RemoteAdmin;
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
    
    public uint[] ReplaceHandlers { get; set; }

    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseItemBinding : IMetaBinding
{
    public ItemAttribute Info { get; set; }
    
    public Type HandlerType { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseRoomBinding : IMetaBinding
{
    public CustomRoomAttribute Info { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseRaCategoryBinding : IMetaBinding
{
    public RaCategoryAttribute Info { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseDataBaseBinding : IMetaBinding
{
    public DatabaseAttribute Info { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}

public class SynapseListenerBinding : IMetaBinding
{
    public Type ListenerType { get; set; }
    
    public IEnumerable<Type> PromisedServices => new Type[] { };
}