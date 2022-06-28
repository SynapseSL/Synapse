using LightContainmentZoneDecontamination;
using Neuron.Core.Meta;

namespace Synapse3.SynapseModule.Map;

public class DecontaminationService : Service
{
    /// <summary>
    /// Returns the Decontamination Controller
    /// </summary>
    public DecontaminationController Controller => DecontaminationController.Singleton;

    /// <summary>
    /// Lock the decontamination progress in its current phase
    /// </summary>
    public bool Locked
    {
        get => Controller._stopUpdating; 
        set => Controller._stopUpdating = value;
    }

    /// <summary>
    /// Returns whether the Decontamination is in Progress
    /// </summary>
    public bool IsDecontaminationInProgress => Controller._decontaminationBegun;

    /// <summary>
    /// Starts the Decontamination
    /// </summary>
    public void InstantStart() => Controller.FinishDecontamination();
}