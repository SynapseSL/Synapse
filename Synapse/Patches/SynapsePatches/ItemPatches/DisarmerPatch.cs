using Harmony;
using System.Linq;
using UnityEngine;

namespace Synapse.Patches.SynapsePatches.ItemPatches
{
    [HarmonyPatch(typeof(Handcuffs),nameof(Handcuffs.IsAliveAndHasDisarmer))]
    internal static class DisarmerPatch
    {
        private static bool Prefix(ref bool __result, GameObject player)
        {
            if(player == null)
            {
                __result = false;
                return false;
            }

            var synapseplayer = player.GetPlayer();
            if (synapseplayer.RoleID != (int)RoleType.Spectator)
                __result = synapseplayer.Inventory.Items.Any(x => x.ID == (int)ItemType.Disarmer);
            else
                __result = false;
            return false;
        }
    }
}
