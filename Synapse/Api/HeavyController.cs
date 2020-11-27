namespace Synapse.Api
{
    public class HeavyController
    {
        internal HeavyController() { }

        public bool ForcedOvercharge => Generator079.mainGenerator.forcedOvercharge;

        public byte ActiveGenerators { get => ForcedOvercharge ? (byte)5 : Generator079.mainGenerator.totalVoltage; internal set => Generator079.mainGenerator.totalVoltage = value; }

        public void Overcharge(bool forced = true) => Recontainer079.BeginContainment(forced);
    }
}
