using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;
using UnityEngine;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "Give",
    Aliases = new[] { "gi", "giveItem" },
    Parameters = new[] { "Player", "ItemID", "(Durability)", "(Attachments)", "(X Size)", "(Y Size)", "(Z Size)" },
    Description = "A Command to give a Player an Item",
    Permission = "synapse.command.give",
    Platforms = new [] { CommandPlatform.RemoteAdmin, CommandPlatform.ServerConsole }
)]
public class GiveCustomItemCommand : SynapseCommand
{
    public override void Execute(SynapseContext context, ref CommandResult result)
    {
        if(context.Arguments.Length < 2)
        {
            result.Response = "To execute this Command provide at least 2 arguments!\nUsage: give[PlayerIDs/PlayerNames] [ID]";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }
        
        if (!Synapse.Get<PlayerService>().TryGetPlayers(context.Arguments[0],out var players,context.Player))
        {
            result.Response = "You didn't input any valid player";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        var durability = 0;
        uint attachments = 0;
        float xSize = 1;
        float ySize = 1;
        float zSize = 1;

        if(context.Arguments.Length > 2)
            if (!int.TryParse(context.Arguments[2], out durability))
            {
                result.Response = "Invalid Parameter for Durability";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }
        
        if (context.Arguments.Length > 3)
            if (!uint.TryParse(context.Arguments[3], out attachments))
            {
                result.Response = "Invalid Parameter for Attachments";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 4)
            if (!float.TryParse(context.Arguments[4], out xSize))
            {
                result.Response = "Invalid Parameter for XSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 5)
            if (!float.TryParse(context.Arguments[5], out ySize))
            {
                result.Response = "Invalid Parameter for YSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 6)
            if (!float.TryParse(context.Arguments[6], out zSize))
            {
                result.Response = "Invalid Parameter for ZSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        var success = false;
        foreach (var player in players)
        {
            var items = context.Arguments[1].Split('.');
            foreach (var itemArgument in items)
            {
                if (!uint.TryParse(itemArgument, out var id)) continue;
                if (!Synapse.Get<ItemService>().IsIdRegistered(id)) continue;
                switch (context.Arguments.Length)
                {
                    case <=2:
                        var item = new SynapseItem(id);
                        item.EquipItem(player, true, true);
                        break;
                    case 3:
                        item = new SynapseItem(id);
                        item.EquipItem(player, true, true);
                        item.Durability = durability;
                        break;
                    case >=4:
                        _ = new SynapseItem(id, player)
                        {
                            Scale = new Vector3(xSize, ySize, zSize),
                            Durability = durability,
                            FireArm =
                            {
                                Attachments = attachments
                            }
                        };  
                        break;
                }

                success = true;
            }
        }

        if (!success)
        {
            result.Response = "You didn't input any items";
            result.StatusCode = CommandStatusCode.BadSyntax;
            return;
        }
        
        result.Response = string.Format("Done! The request affected {0} player{1}", players.Count.ToString(),
            players.Count == 1 ? "!" : "s!");
        result.StatusCode = CommandStatusCode.Ok;
    }
}