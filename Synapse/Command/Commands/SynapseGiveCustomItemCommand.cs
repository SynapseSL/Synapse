using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "GiveItem",
        Aliases = new string[] {"gi"},
        Description = "A Command to give a Player an Item",
        Permission = "synapse.command.give",
        Platforms = new[] {Platform.RemoteAdmin,Platform.ServerConsole},
        Usage = "give player id or give player id durabillity sight barrel other xsize ysize zsize"
        )]
    public class SynapseGiveCustomItemCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (!context.Player.HasPermission("synapse.command.give"))
            {
                result.Message = "You dont have permission to execute this command!";
                result.State = CommandResultState.NoPermission;
                return result;
            }

            if(context.Arguments.Count < 2)
            {
                result.Message = "Missing parameter! Command Usage: give player itemid";
                result.State = CommandResultState.Error;
                return result;
            }

            var player = Server.Get.GetPlayer(context.Arguments.FirstElement());
            if(player == null)
            {
                result.Message = "No Player was found";
                result.State = CommandResultState.Error;
                return result;
            }

            if(!int.TryParse(context.Arguments.ElementAt(1),out var id))
            {
                result.Message = "Invalid Parameter for ItemID";
                result.State = CommandResultState.Error;
                return result;
            }

            float durabillity = 0;
            int sight = 0;
            int barrel = 0;
            int other = 0;
            float xsize = 1;
            float ysize = 1;
            float zsize = 1;

            if(context.Arguments.Count > 2)
                if(!float.TryParse(context.Arguments.ElementAt(2),out durabillity))
                {
                    result.Message = "Invalid Parameter for Durabillity";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (context.Arguments.Count > 3)
                if (!int.TryParse(context.Arguments.ElementAt(3), out sight))
                {
                    result.Message = "Invalid Parameter for Sight";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (context.Arguments.Count > 4)
                if (!int.TryParse(context.Arguments.ElementAt(4), out barrel))
                {
                    result.Message = "Invalid Parameter for Barrel";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (context.Arguments.Count > 5)
                if (!int.TryParse(context.Arguments.ElementAt(5), out other))
                {
                    result.Message = "Invalid Parameter for Other";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (context.Arguments.Count > 6)
                if (!float.TryParse(context.Arguments.ElementAt(6), out xsize))
                {
                    result.Message = "Invalid Parameter for XSize";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (context.Arguments.Count > 7)
                if (!float.TryParse(context.Arguments.ElementAt(7), out ysize))
                {
                    result.Message = "Invalid Parameter for YSize";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (context.Arguments.Count > 8)
                if (!float.TryParse(context.Arguments.ElementAt(8), out zsize))
                {
                    result.Message = "Invalid Parameter for ZSize";
                    result.State = CommandResultState.Error;
                    return result;
                }

            if (!Server.Get.ItemManager.IsIDRegistered(id))
            {
                result.Message = "No Item with this ItemId was found";
                result.State = CommandResultState.Error;
                return result;
            }

            var item = new Api.Items.SynapseItem(id, durabillity, sight, barrel, other);
            item.Scale = new UnityEngine.Vector3(xsize, ysize, zsize);
            player.Inventory.AddItem(item);


            result.Message = "Added Item to Players Inventory!";
            result.State = CommandResultState.Ok;

            return result;
        }
    }
}
