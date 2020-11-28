namespace Synapse.Api
{
    public class HeavyController
    {
        internal HeavyController() { }

        public bool ForcedOvercharge => Generator079.mainGenerator.forcedOvercharge;

        public byte ActiveGenerators { get => ForcedOvercharge ? (byte)5 : Generator079.mainGenerator.totalVoltage; internal set => Generator079.mainGenerator.totalVoltage = value; }

        public void Overcharge(bool forced = true)
        {
            if (forced)
                Generator079.mainGenerator.forcedOvercharge = true;
            Recontainer079.BeginContainment(forced);
        }

        public void LightsOut(float duration, bool onlyHeavy = true) => Generator079.mainGenerator.ServerOvercharge(duration, onlyHeavy);
    }
}
