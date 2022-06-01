using System.Linq;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "GiveItem",
        Aliases = new string[] { "gi" },
        Description = "A Command to give a Player an Item",
        Permission = "synapse.command.give",
        Platforms = new[] { Platform.RemoteAdmin, Platform.ServerConsole },
        Usage = "give player id or give player id durabillity sight barrel other xsize ysize zsize",
        Arguments = new[] { "Player", "ItemID", "(Durabillity)", "(Attachments)", "(X Size)", "(Y Size)", "(Z Size)" }
        )]
    public class SynapseGiveCustomItemCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count < 2)
            {
                result.Message = "Missing parameter! Command Usage: give player itemid";
                result.State = CommandResultState.Error;
                return result;
            }

            var player = Server.Get.GetPlayer(context.Arguments.FirstElement());
            if (player is null)
            {
                result.Message = "No Player was found";
                result.State = CommandResultState.Error;
                return result;
            }

            if (!System.Int32.TryParse(context.Arguments.ElementAt(1), out var id))
            {
                result.Message = "Invalid Parameter for ItemID";
                result.State = CommandResultState.Error;
                return result;
            }

            float durabillity = 0;
            uint sight = 0;
            float xsize = 1;
            float ysize = 1;
            float zsize = 1;

            if (context.Arguments.Count > 2)
            {
                if (!System.Single.TryParse(context.Arguments.ElementAt(2), out durabillity))
                {
                    result.Message = "Invalid Parameter for Durabillity";
                    result.State = CommandResultState.Error;
                    return result;
                }
            }

            if (context.Arguments.Count > 3)
            {
                if (!System.UInt32.TryParse(context.Arguments.ElementAt(3), out sight))
                {
                    result.Message = "Invalid Parameter for Attachements";
                    result.State = CommandResultState.Error;
                    return result;
                }
            }

            if (context.Arguments.Count > 4)
            {
                if (!System.Single.TryParse(context.Arguments.ElementAt(4), out xsize))
                {
                    result.Message = "Invalid Parameter for XSize";
                    result.State = CommandResultState.Error;
                    return result;
                }
            }

            if (context.Arguments.Count > 5)
            {
                if (!System.Single.TryParse(context.Arguments.ElementAt(5), out ysize))
                {
                    result.Message = "Invalid Parameter for YSize";
                    result.State = CommandResultState.Error;
                    return result;
                }
            }

            if (context.Arguments.Count > 6)
            {
                if (!System.Single.TryParse(context.Arguments.ElementAt(6), out zsize))
                {
                    result.Message = "Invalid Parameter for ZSize";
                    result.State = CommandResultState.Error;
                    return result;
                }
            }

            if (!Server.Get.ItemManager.IsIDRegistered(id))
            {
                result.Message = "No Item with this ItemId was found";
                result.State = CommandResultState.Error;
                return result;
            }

            var item = new Api.Items.SynapseItem(id)
            {
                Scale = new UnityEngine.Vector3(xsize, ysize, zsize),
            };
            player.Inventory.AddItem(item);
            item.Durabillity = durabillity;
            item.WeaponAttachments = sight;

            result.Message = "Added Item to Players Inventory!";
            result.State = CommandResultState.Ok;

            return result;
        }
    }
}
