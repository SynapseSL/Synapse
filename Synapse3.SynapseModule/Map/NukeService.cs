using Mirror;
using Neuron.Core.Meta;
using Synapse3.SynapseModule.Enums;
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

    public float TimeUntilDetonation
    {
        get => AlphaWarheadController.TimeUntilDetonation;
        set => WarheadController.ForceTime(value);
    }

    public int DeathsByNuke => WarheadController.WarheadKills;

    public NukeState State
    {
        get
        {
            if (WarheadController._alreadyDetonated) return NukeState.Detonated;

            if (WarheadController.Info.InProgress) return NukeState.Active;

            return InsidePanel.Enabled ? NukeState.Prepared : NukeState.Inactive;
        }
    }

    public bool CanDetonate => !WarheadController.Info.InProgress &&
                               WarheadController.CooldownEndTime <= NetworkTime.time && !WarheadController.IsLocked;

    public void StartDetonation()
    {
        WarheadController.InstantPrepare();
        WarheadController.StartDetonation();
    }

    public void CancelDetonation()
        => WarheadController.CancelDetonation();

    public void Shake() => WarheadController.RpcShake(true);

    public void InstantDetonation() => WarheadController.ForceTime(0f);

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
            OutsidePanel = Synapse.GetObject<AlphaWarheadOutsitePanel>();
        }

        public bool KeyCardEntered
        {
            get => OutsidePanel.NetworkkeycardEntered;
            set => OutsidePanel.NetworkkeycardEntered = value;
        }
    }
}