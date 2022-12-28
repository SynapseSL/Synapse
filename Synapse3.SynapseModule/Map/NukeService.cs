using Neuron.Core.Meta;
using Synapse3.SynapseModule.Events;
using UnityEngine;

namespace Synapse3.SynapseModule.Map;

public class NukeService : Service
{
    private RoundEvents _round;

    public NukeService(RoundEvents round)
    {
        _round = round;
    }

    public override void Enable()
    {
        _round.Waiting.Subscribe(SetController);
    }

    public override void Disable()
    {
        _round.Waiting.Unsubscribe(SetController);
    }

    private void SetController(RoundWaitingEvent ev)
    {
        WarheadController = AlphaWarheadController.Singleton;
        InsidePanel.SetPanel();
        OutsidePanel.SetPanel();
    }

    public AlphaWarheadController WarheadController { get; private set; }

    public NukeInsidePanel InsidePanel { get; } = new();

    public NukeOutsidePanel OutsidePanel { get; } = new();

    
    //TODO:
    /*
    public float TimeUntilDetonation
    {
        get => WarheadController.NetworktimeToDetonation;
        set => WarheadController.NetworktimeToDetonation = value;
    }

    public int DeathsByNuke => WarheadController.warheadKills;

    public NukeState State
    {
        get
        {
            if (WarheadController.detonated) return NukeState.Detonated;

            if (WarheadController.NetworkinProgress) return NukeState.Active;

            if (InsidePanel.Enabled) return NukeState.Prepared;
            
            return NukeState.Inactive;
        }
    }
    

    public bool CanDetonate
        => WarheadController.CanDetonate;
        */
    
    public void StartDetonation()
        => WarheadController.StartDetonation();

    public void CancelDetonation()
        => WarheadController.CancelDetonation();

    //TODO:
    /*
    public void Shake()
    {
        foreach (var synapsePlayer in Synapse.Get<PlayerService>().Players)
        {
            WarheadController.TargetRpcShake(synapsePlayer.Connection, true, false);
        }
    }

    public void InstantDetonation()
    {
        WarheadController.InstantPrepare();
        WarheadController.StartDetonation(false, true);
        WarheadController.NetworktimeToDetonation = 0.1f;
    }
    */

    public class NukeInsidePanel
    {
        public AlphaWarheadNukesitePanel NukeSitePanel { get; private set; }
        
        internal void SetPanel()
        {
            NukeSitePanel = AlphaWarheadOutsitePanel.nukeside;
        }

        public bool Enabled
        {
            get => NukeSitePanel.Networkenabled;
            set => NukeSitePanel.Networkenabled = value;
        }
        
        public bool Locked { get; set; }

        public Transform Level => NukeSitePanel.lever;

        public Vector3 Position => NukeSitePanel.lever.position;
    }
    
    public class NukeOutsidePanel
    {
        public AlphaWarheadOutsitePanel OutsidePanel { get; private set; }

        internal void SetPanel()
        {
            OutsidePanel = Synapse.Get<AlphaWarheadOutsitePanel>();
        }

        public bool KeyCardEntered
        {
            get => OutsidePanel.NetworkkeycardEntered;
            set => OutsidePanel.NetworkkeycardEntered = value;
        }
    }
}