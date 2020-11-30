using System;
using Mirror;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Synapse.Api;
using Synapse.Api.Items;
using Event = Synapse.Api.Events.EventHandler;

namespace Synapse.Patches.EventsPatches.MapPatches
{
    [HarmonyPatch(typeof(Scp914.Scp914Machine),nameof(Scp914.Scp914Machine.ProcessItems))]
    internal static class Class1
    {
        private static bool Prefix(Scp914.Scp914Machine __instance)
        {
            try
            {
				if (!NetworkServer.active)
					return false;

				var array = Physics.OverlapBox(__instance.intake.position, __instance.inputSize / 2f);
				var players = new List<Player>();
				var items = new List<SynapseItem>();

				foreach (var collider in array)
				{
					var player = collider.GetComponent<Player>();
					if (player != null)
						players.Add(player);
					else
					{
						var item = collider.GetComponent<Pickup>().GetSynapseItem();
						if (item != null)
							items.Add(item);
					}
				}

				Event.Get.Map.Invoke914Activate(ref players, ref items, out var allow, out var move);

				if (!allow) return false;

				var vanillaitems = items.Where(x => x.State == Api.Enum.ItemState.Map).Select(x => x.pickup);
				var vanillaplayers = players.Select(x => x.ClassManager);

				try
				{
					if (move)
						__instance.MoveObjects(vanillaitems, vanillaplayers);
				}
				finally
				{
					__instance.UpgradeObjects(vanillaitems, vanillaplayers.ToList());
				}

				return false;
			}
            catch (Exception e)
            {
				Synapse.Api.Logger.Get.Error($"Synapse-Event: Scp914 Activate Event failed!!\n{e}");
				return true;
            }
        }
    }
}
