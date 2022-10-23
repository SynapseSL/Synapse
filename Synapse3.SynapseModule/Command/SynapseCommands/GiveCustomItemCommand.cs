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
            result.Response = "Missing parameter! Command Usage: give player itemId";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }
        
        if (!Synapse.Get<PlayerService>().TryGetPlayers(context.Arguments[0],out var players,context.Player))
        {
            result.Response = "No Valid Player was found/selected";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if (!uint.TryParse(context.Arguments[1], out var id)) 
        {
            result.Response = "Invalid Parameter for ItemID";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        var durabillity = 0;
        uint attachments = 0;
        float xsize = 1;
        float ysize = 1;
        float zsize = 1;

        if(context.Arguments.Length > 2)
            if (!int.TryParse(context.Arguments[2], out durabillity))
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
            if (!float.TryParse(context.Arguments[4], out xsize))
            {
                result.Response = "Invalid Parameter for XSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 5)
            if (!float.TryParse(context.Arguments[5], out ysize))
            {
                result.Response = "Invalid Parameter for YSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 6)
            if (!float.TryParse(context.Arguments[6], out zsize))
            {
                result.Response = "Invalid Parameter for ZSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (!Synapse.Get<ItemService>().IsIdRegistered(id))
        {
            result.Response = "No Item with this ItemId was found";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        foreach (var player in players)
        {
            switch (context.Arguments.Length)
            {
                case <=2:
                    var item = new SynapseItem(id);
                    item.EquipItem(player, true, true);
                    break;
                case 3:
                    item = new SynapseItem(id);
                    item.EquipItem(player, true, true);
                    item.Durability = durabillity;
                    break;
                case >=4:
                    _ = new SynapseItem(id, player)
                    {
                        Scale = new Vector3(xsize, ysize, zsize),
                        Durability = durabillity,
                        FireArm =
                        {
                            Attachments = attachments
                        }
                    };  
                    break;
            }
        }

        result.Response = string.Format("Done! The request affected {0} player{1}", players.Count.ToString(),
            players.Count == 1 ? "!" : "s!");
        result.StatusCode = CommandStatusCode.Ok;
    }
}