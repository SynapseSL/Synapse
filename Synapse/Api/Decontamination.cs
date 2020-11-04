using LightContainmentZoneDecontamination;
using Mirror;

namespace Synapse.Api
{
    public class Decontamination
    {
        internal Decontamination() { }

        /// <summary>
        /// Returns the Decontamination Controller
        /// </summary>
        public DecontaminationController Controller => DecontaminationController.Singleton;

        /// <summary>
        /// Returns whether the Decontamination is in Progress
        /// </summary>
        public bool IsDecontaminationInProgress => Controller._decontaminationBegun;

        /// <summary>
        /// Starts the Decontamination
        /// </summary>
        public void InstantStart() => Controller.FinishDecontamination();
    }
}