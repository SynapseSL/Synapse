using Harmony;

namespace Synapse.Patches.SynapsePatches
{
    [HarmonyPatch(typeof(RoomManager), nameof(RoomManager.GenerateMap))]
    internal class GenerateMapPatch
    {
        private static void Postfix(RoomManager __instance)
        {
            SynapseController.Server.Map.Rooms.Clear();
            foreach (var room in __instance.rooms)
                SynapseController.Server.Map.Rooms.Add(new Api.Room(room));
        }
    }
}
