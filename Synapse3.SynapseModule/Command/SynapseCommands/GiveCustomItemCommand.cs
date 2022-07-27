using System.Linq;
using Neuron.Modules.Commands;
using Neuron.Modules.Commands.Command;
using Synapse3.SynapseModule.Item;
using Synapse3.SynapseModule.Player;

namespace Synapse3.SynapseModule.Command.SynapseCommands;

[SynapseRaCommand(
    CommandName = "GiveItem",
    Aliases = new[] { "gi" },
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
            result.Response = "No Player was found";
            result.StatusCode = CommandStatusCode.Error;
            return;
        }

        if (!int.TryParse(context.Arguments[1], out var id)) 
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
            if (!int.TryParse(context.Arguments.ElementAt(3), out durabillity))
            {
                result.Response = "Invalid Parameter for Durability";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }
        
        if (context.Arguments.Length > 3)
            if (!uint.TryParse(context.Arguments.ElementAt(4), out attachments))
            {
                result.Response = "Invalid Parameter for Attachments";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 4)
            if (!float.TryParse(context.Arguments.ElementAt(5), out xsize))
            {
                result.Response = "Invalid Parameter for XSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 5)
            if (!float.TryParse(context.Arguments.ElementAt(6), out ysize))
            {
                result.Response = "Invalid Parameter for YSize";
                result.StatusCode = CommandStatusCode.Error;
                return;
            }

        if (context.Arguments.Length > 6)
            if (!float.TryParse(context.Arguments.ElementAt(7), out zsize))
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
            _ = new SynapseItem(id, player)
            {
                Scale = new UnityEngine.Vector3(xsize, ysize, zsize),
                Durability = durabillity,
                FireArm =
                {
                    Attachments = attachments
                }
            };   
        }

        result.Response = "Added Item to Players Inventory!";
        result.StatusCode = CommandStatusCode.Ok;
    }
}