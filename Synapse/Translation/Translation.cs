using System.ComponentModel;

namespace Synapse.Translation
{
    internal class Translation : IPluginTranslation
    {
        [Description("The message that is displayed when a Custom Role harms a person that he can't harm")]
        public string sameTeam = "<b>You can't harm this person</b>";

        [Description("The message that appear when you do something that hurt a SCP if you cant hurt a SCP")]
        public string scpTeam = "As your current Role you can't harm an Scp";

        [Description("The message that apperas if a player executes a command to which he has no permissions")]
        public string noPermissions = "You don't have permission to execute this command (%perm%)";
    }
}
