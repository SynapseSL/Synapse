using System;
using Harmony;

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
	public class WaitingForPlayersPatch
	{
		public static void Prefix(ref string q)
		{
			try
			{
                if (q.StartsWith("Round finished! Anomalies: "))
					Server.Get.Events.Round.InvokeRoundEndEvent();
				else if (q == "Waiting for players...")
                {
					SynapseController.Server.Map.AddObjects();
					SynapseController.Server.Map.Round.CurrentRound++;
					SynapseController.Server.Events.Round.InvokeWaitingForPlayers();
                }
			}
			catch (Exception e)
			{
				SynapseController.Server.Logger.Error($"Synapse-Event: WaitingForPlayers failed!!\n{e}");
			}
		}
	}
}
