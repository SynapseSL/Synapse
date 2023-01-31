using System.Collections.Generic;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

public abstract class RemoteAdminCategory : InjectedLoggerBase
{
    public RaCategoryAttribute Attribute { get; set; }

    public abstract string GetInfo(CommandSender sender, bool secondPage);

    public abstract List<SynapsePlayer> GetPlayers();

    public abstract bool DisplayOnTop { get; }

    public abstract bool CanSeeCategory(SynapsePlayer player);

    public virtual string ExternalURL => "";

    public virtual void Load() { }
}