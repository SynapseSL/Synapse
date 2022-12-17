using Neuron.Modules.Commands;
using Synapse3.SynapseModule.Enums;

namespace Synapse3.SynapseModule.Command;

public class RemoteAdminAttachment : IAttachment
{
    public string DisplayName { get; set; } = "";

    public RaCategory DisplayCategory { get; set; } = RaCategory.None;
}