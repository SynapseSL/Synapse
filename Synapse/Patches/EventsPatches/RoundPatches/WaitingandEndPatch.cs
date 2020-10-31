using System;
using HarmonyLib;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.RoundPatches
{

	/// <summary>
	/// The WaitingForPlayers Patch is a special one.
	///
	/// This solution is currently the only way to check if a server is finished with initialisation or not.
	/// Since the "Init"-Method (where the Server States that it is indeed "ready for players" is an IEnumerator, Harmony didn't really have a way of patching
	/// the IEnumerator. Until Northwood does a better Handling of this, this is here to stay.
	/// </summary>
	[HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
	public class WaitingandEndPatch
	{
		public static void Prefix(ref string q)
		{
			if (q.StartsWith("Round finished! Anomalies: "))
				try
				{
					Server.Get.Events.Round.InvokeRoundEndEvent();
				}
				catch (Exception e)
				{
					SynapseController.Server.Logger.Error($"Synapse-Event: RoundEnd failed!!\n{e}");
				}
			else if (q == "Waiting for players...")
			{
				try
				{
					SynapseController.Server.Map.AddObjects();
					SynapseController.Server.Map.Round.CurrentRound++;
					SynapseController.Server.Events.Round.InvokeWaitingForPlayers();
				}
				catch (Exception e)
				{
					SynapseController.Server.Logger.Error($"Synapse-Event: WaitingForPlayers failed!!\n{e}");
				}
			}
		}
	}
}
