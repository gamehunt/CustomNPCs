using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.ComponentModel;
using System.IO;

namespace NPCS
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Enables debug output (Spams trash in console)")]
        public bool VerboseOutput { get; set; } = false;

        [Description("Should PlayerList contain NPCs?")]
        public bool DisplayNpcInPlayerList { get; set; } = false;

        [Description("If false NPCs will be cleaned if they are alone on server")]
        public bool AllowAloneNpcs { get; set; } = true;

        [Description("Maximum distance between NPC and follow target. If it's reached NPC will tp to target")]
        public float MaxFollowDistance { get; set; } = 15f;

        //TRANSLATIONS

        [Description("Localizations")]

        public string TranslationAlreadyTalking { get; set; } = "We are already talking!";
        public string TranslationNpcBusy { get; set; } = "I'm busy now, wait a second";
        public string TranslationTalkEnd { get; set; } = "ended talk";
        public string TranslationInvalidAnswer { get; set; } = "Invalid answer!";
        public string TranslationIncorrectFormat { get; set; } = "Incorrect answer format!";
        public string TranslationNotTalking { get; set; } = "You aren't talking to this NPC!";
        public string TranslationNpcNotFound { get; set; } = "NPC not found!";
        public string TranslationAnswerNumber { get; set; } = "You must provide answer number!";
        public string TranslationOnlyPlayers { get; set; } = "Only players can use this!";
        public string TranslationBanBroadcast { get; set; } = "<color=red>DONT BAN OR KICK NPCs</color>";


        public static string NPCs_root_path = Path.Combine(Paths.Configs, "npcs");

        public static string NPCs_nodes_path = Path.Combine(NPCs_root_path, "nodes");


        public static string DefaultNodeContents =
@"---
description: NOPE
reply: HELLO THERE
conditions: []
actions: []
next_nodes: []";

        public static string DefaultNPCContents =
@"---
name: DEFAULT
role: 6
health: -1
item_held: 24
root_node: default_node.yml
god_mode: true
is_exclusive: true
events: []
";
    }
}