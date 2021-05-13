using System.Text.RegularExpressions;
using HarmonyLib;
using Synapse.Database;

namespace Synapse.Command.Commands
{
    [CommandInformation(
        Name = "NetworkPassword",
        Aliases = new[] {"netpass"},
        Description = "Sets your network password",
        Usage = "netpass <password> ",
        Permission = "synapse.command.networkpassword",
        Platforms = new[] {Platform.RemoteAdmin, Platform.ServerConsole}
    )]
    public class SynapseNetworkPasswordCommand : ISynapseCommand
    {
        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if (context.Arguments.Count >= 1)
            {
                DatabaseManager.CheckEnabledOrThrow();
                var password = context.Arguments.Join(delimiter: " ");

                if (Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*[!@#$&*])(?=.*[0-9])(?=.*[a-z]).{8,}$"))
                {
                    result.Message =
                        "Your password must be at least 8 characters, have one lowercase and one uppercase letters, " +
                        "as well as a one digit and special character '!@#$&*'. Also remember to use a new password for " +
                        "Synapse since this password is stored in PLAINTEXT";
                    result.State = CommandResultState.Error;
                }


                context.Player.SetData("netpass", password);
            }
            else
            {
                result.Message = "Please specify a password";
                result.State = CommandResultState.Error;
                return result;
            }

            result.Message =
                "Password set, please remember to use a new password for Synapse servers since passwords are stored in PLAINTEXT, " +
                "in order to make the client endpoint authentication actually implementable since we can't use ssl because auf EmbedIO " +
                "limitations requiring us to implement some own encryption. If you use this password on some other account, " +
                "PLEASE CHANGE YOUR PASSWORD here by using 'netpass <password>' again";
            return result;
        }
    }
}