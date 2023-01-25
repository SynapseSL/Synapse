using System;
using System.ComponentModel;
using Neuron.Core.Meta;
using Neuron.Modules.Configs.Localization;

namespace Synapse3.SynapseModule.Config;

[Automatic]
[Serializable]
public class SynapseTranslation : Translations<SynapseTranslation>
{
    public string ScpTeam { get; set; } = "As SCP are you not able to cause Harm towards another SCP";
    public string SameTeam { get; set; } = "This Person is in your Team therefore can you not harm it!";


    [Description("Displays a Broadcast when a Player joins the Server. Leave empty for none")]
    public string Broadcast { get; set; } = string.Empty;
    public ushort BroadcastDuration { get; set; } = 5;

    [Description("Displays a Hint when a Player joins the Server. Leave empty for none")]
    public string Hint { get; set; } = string.Empty;
    public float HintDuration { get; set; } = 5;

    [Description("Opens a Report Window and displays the Message when a Player joins the Server. Leave empty for none")]
    public string Window { get; set; } = string.Empty;


    public string TranslationCommandNoTranslation { get; set; } = "No Translation was found for you. Type .language {Language here} in your console to set your Language";
    public string TranslationCommandGetTranslation { get; set; } = "Your current Translation is: {0}";
    public string TranslationCommandSetTranslation { get; set; } = "Your Translation is set to: {0}";

    public string DnT { get; set; } =
        "You have enabled Do Not Track in your settings therefore can we not store your language";

    public string DeathMessage { get; set; } = "You were killed by\n{0}\nas\n{1}";

    public string KeyBindCommandGetCommand { get; set; } = "Select a command whose bind you want to change, current bind:{0}";
    public string KeyBindCommandSelectKey { get; set; } = "Please press the new key";
    public string KeyBindSet { get; set; } = "{0} was binded to {1}";
    public string KeyBindCommandInvalidKey { get; set; } = "No Bind with that name was found";

    public string CommandHelp { get; set; } = "All Available Vanilla Commands:";
    public string CommandHelpSecond { get; set; } = "All Available Synapse Commands:";
    public string CommandNotFound { get; set; } = "No command with that name was found";

    public string DisableProximity { get; set; } = "Proximity Voice Chat has been disabled";
    public string EnableProximity { get; set; } = "Proximity Voice Chat has been enabled";
    public string NotScpProximity { get; set; } = "You are not an SCP and therefore can't use the SCP Proximity Chat";
    public string NotAllowedProximity { get; set; } = "You are not allowed to use the Proximity Chat";
}
