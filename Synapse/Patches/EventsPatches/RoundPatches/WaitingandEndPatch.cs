﻿using System;
using HarmonyLib;

// ReSharper disable All
namespace Synapse.Patches.EventsPatches.RoundPatches
{
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
					SynapseController.Server.Logger.Error($"Synapse-Event: WaitingForPlayers failed!!\n{e}\nStackTrace:\n{e.StackTrace}");
				}
			}
		}
	}
}
