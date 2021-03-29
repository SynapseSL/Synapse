namespace Synapse.Api
{
    public class HeavyController
    {
        public static HeavyController Get => Map.Get.HeavyController;

        internal HeavyController() { }

        public bool ForcedOvercharge => Generator079.mainGenerator.forcedOvercharge;

        public byte ActiveGenerators { get => ForcedOvercharge ? (byte)5 : Generator079.mainGenerator.totalVoltage; internal set => Generator079.mainGenerator.totalVoltage = value; }

        public bool Is079Recontained { get; internal set; } = false;

        public void Recontain079(bool forced = true) => Recontainer079.BeginContainment(forced);

        public void Overcharge(bool forced = true)
        {
            if (forced)
            {
                NineTailedFoxAnnouncer.singleton.ServerOnlyAddGlitchyPhrase("ALLSECURED . SCP 0 7 9 RECONTAINMENT SEQUENCE COMMENCING . FORCEOVERCHARGE", 0.1f, 0.07f);
                Generator079.mainGenerator.forcedOvercharge = true;
                Recontain079(forced);
            }
            else
                foreach (var gen in Map.Get.Generators)
                    if (!gen.IsOvercharged)
                        gen.Overcharge();
        }

        public void LightsOut(float duration, bool onlyHeavy = true) => Generator079.mainGenerator.ServerOvercharge(duration, onlyHeavy);
    }
}
