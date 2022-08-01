using System.Collections.Generic;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Permissions.RemoteAdmin;

public abstract class RemoteAdminCategory
{
    public RaCategoryAttribute Attribute { get; set; }

    public abstract string GetInfo(CommandSender sender, bool secondPage);

    public abstract List<SynapsePlayer> GetPlayers();

    public abstract bool DisplayOnTop { get; }

    public abstract bool CanSeeCategory(SynapsePlayer player);
}