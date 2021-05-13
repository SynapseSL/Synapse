using Synapse.Api;

namespace Synapse.Network.Models
{
    public class NetRound
    {
        public int MTFs { get; set; }
        public int Researchers { get; set; }
        public int DClasses { get; set; }
        public int Chaos { get; set; }
        public int SCPs { get; set; }
        public int Dead { get; set; }

        public bool Warhead { get; set; }
        public bool Decontamination { get; set; }

        public int RoundLength { get; set; }

        public int RoundSinceRestart { get; set; }


        public static NetRound Get()
        {
            return new NetRound
            {
                MTFs = Server.Get.GetPlayers(e => e.Team == Team.MTF).Count,
                Researchers = Server.Get.GetPlayers(e => e.Team == Team.RSC).Count,
                DClasses = Server.Get.GetPlayers(e => e.Team == Team.CDP).Count,
                Chaos = Server.Get.GetPlayers(e => e.Team == Team.CHI).Count,
                SCPs = Server.Get.GetPlayers(e => e.Team == Team.SCP).Count,
                Dead = Server.Get.GetPlayers(e => e.Team == Team.RIP).Count,

                Warhead = Map.Get.Nuke.Detonated,
                Decontamination = Map.Get.Decontamination.Locked,

                RoundLength = (int) Map.Get.Round.RoundLength.TotalSeconds,
                RoundSinceRestart = Map.Get.Round.CurrentRound
            };
        }
    }
}