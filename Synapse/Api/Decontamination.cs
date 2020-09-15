using LightContainmentZoneDecontamination;
using Mirror;

namespace Synapse.Api
{
    public class Decontamination
    {
        /// <summary>
        /// Gives you the Decontamination Controller
        /// </summary>
        public DecontaminationController Controller => DecontaminationController.Singleton;

        /// <summary>
        /// Is the Decontamination Countdown disabled?
        /// </summary>
        public bool DisableDecontamination
        {
            get => Controller._disableDecontamination;
            set
            {
                if (value)
                {
                    Controller._stopUpdating = false;
                    Controller.RoundStartTime = NetworkTime.time;
                }
                Controller._disableDecontamination = value;
            }
        }

        /// <summary>
        /// Is the Decontamination in Progress?
        /// </summary>
        public bool IsDecontaminationInProgress => Controller._decontaminationBegun;

        /// <summary>
        ///  Starts the Decontamination
        /// </summary>
        public void InstantDecontamination() => Controller.FinishDecontamination();
    }
}