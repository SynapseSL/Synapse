using System;

namespace Synapse.Api
{
    public class HeavyController
    {
        public static HeavyController Get => Map.Get.HeavyController;

        internal HeavyController() { }

        private Recontainer079 Container => Server.Get.GetObjectOf<Recontainer079>();

        public byte ActiveGenerators => (byte)Container._prevEngaged;

        public bool Is079Recontained
        {
            get
            {
                var container = Container;
                return Container._alreadyRecontained && Container._delayStopwatch.Elapsed.TotalSeconds > container._activationDelay;
            }
        }

        public void Recontain079()
        {
            var recontainer = Container;

            _ = recontainer.TryKill079();
            recontainer.PlayAnnouncement(recontainer._announcementSuccess + " Unknown", 1f);
        }

        public void Overcharge() => Container.Recontain();

        public void LightsOut(float duration, bool onlyHeavy = true)
        {
            foreach (var room in Map.Get.Rooms)
            {
                if (!onlyHeavy || room.Zone == Enum.ZoneType.HCZ)
                    room.LightController?.ServerFlickerLights(duration);
            }
        }

        [Obsolete("You don't need forced any more")]
        public void Overcharge(bool forced = true) => Overcharge();

        [Obsolete("You don't need forced any more")]
        public void Recontain079(bool forced = true) => Recontain079();

        [Obsolete("Removed Since 11.0.0")]
        public bool ForcedOvercharge => false;
    }
}
