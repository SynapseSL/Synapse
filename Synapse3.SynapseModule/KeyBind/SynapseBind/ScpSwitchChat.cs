using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CmdBinding;

namespace Synapse3.SynapseModule.KeyBind.SynapseBind;

[Automatic]
[KeyBind(
    Bind = UnityEngine.KeyCode.V,
    CommandName = "ScpChat",
    CommandDescription = "Changes between scp and proximity chat when you are a talking scp"
    )]
internal class ScpSwitchChat : SynapseAbstractKeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        if (!player.ScpController.CanTalk) return;

        player.ScpController.ProximityChat = !player.ScpController.ProximityChat;
    }
}
