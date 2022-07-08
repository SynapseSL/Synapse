using MEC;
using Mirror;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Dummy;

public class DummyPlayer : SynapsePlayer
{
    public override PlayerType PlayerType => PlayerType.Dummy;

    public override RoleType RoleType
    {
        set
        {
            NetworkServer.UnSpawn(gameObject);
            ClassManager.CurClass = value;
            NetworkServer.Spawn(gameObject);
        }
    }
    
    public override void Awake()
    {
        var service = Synapse.Get<DummyService>();
        //This need to wait one Frame or else it will be executed before Synapse can set SynapseDummy
        Timing.CallDelayed(Timing.WaitForOneFrame, () =>
        {
            if (service._dummies.Contains(SynapseDummy)) return;

            service._dummies.Add(SynapseDummy);
        });
    }

    public override void OnDestroy()
    {
        var service = Synapse.Get<DummyService>();
        service._dummies.Remove(SynapseDummy);
    }
    
    public SynapseDummy SynapseDummy { get; internal set; }
}