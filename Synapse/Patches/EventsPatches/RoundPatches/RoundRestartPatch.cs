using Synapse.Api;

namespace Synapse.Patches.EventsPatches.RoundPatches
{
    internal static class RoundRestartPatch
    {
        private static void Prefix()
        {
            var map = Map.Get;
            map.Teslas.Clear();
            map.Doors.Clear();
            map.Elevators.Clear();
            map.Rooms.Clear();
            map.Generators.Clear();

            //TODO: RestartEvent
        }
    }
}
