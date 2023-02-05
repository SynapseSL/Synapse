#if DEV
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.KeyBind.SynapseBind;

[KeyBind(
    Bind = KeyCode.Alpha1,
    CommandName = "Dev",
    CommandDescription = "Dev Bind"
    )]
public class Dev : KeyBind
{
    public override void Execute(SynapsePlayer player)
    {
        Synapse.Get<ReferenceHub>();
    }
}
#endif