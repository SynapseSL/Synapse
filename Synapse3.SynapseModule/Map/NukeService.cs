using Mirror;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
using Synapse3.SynapseModule.Events;
using Synapse3.SynapseModule.Player;
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
    
    /// <summary>
    /// Change the Time until the detonantion, start the count down if not started
    /// </summary>
    public double TimeUntilDetonation
    {
        get
        {
            var info = AlphaWarheadController.Singleton.NetworkInfo;
            return info.StartTime - NetworkTime.time;
        }
        set
        {
            var info = AlphaWarheadController.Singleton.NetworkInfo;
            info.StartTime = value;
            AlphaWarheadController.Singleton.NetworkInfo = info;
        }
    }

    public int DeathsByNuke => WarheadController.WarheadKills;

    public NukeState State
    {
        get
        {
            if (AlphaWarheadController.Detonated) return NukeState.Detonated;

            if (AlphaWarheadController.InProgress) return NukeState.Active;

            if (InsidePanel.Enabled) return NukeState.Prepared;
            
            return NukeState.Inactive;
        }
    }
    

    public bool CanDetonate
        => AlphaWarheadController.Detonated;
        
    
    public void StartDetonation()
        => WarheadController.StartDetonation();

    public void CancelDetonation()
        => WarheadController.CancelDetonation();

    public void Shake()
    {
        WarheadController.RpcShake(false);
    }

    public void InstantDetonation()
    {
        WarheadController.InstantPrepare();
        WarheadController.StartDetonation(false, true);
        TimeUntilDetonation = 0.1f;
    }


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