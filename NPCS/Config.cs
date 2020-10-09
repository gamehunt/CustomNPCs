using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace NPCS
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("Enables debug output (Spams trash in console)")]
        public bool VerboseOutput { get; set; } = false;

        [Description("Should RA contain NPCs?")]
        public bool DisplayNpcInRemoteAdmin { get; set; } = false;

        [Description("Maximum distance between NPC and follow target. If it's reached NPC will tp to target")]
        public float MaxFollowDistance { get; set; } = 15f;

        [Description("Enable this if you will use nav system (not neccessary for following)")]
        public bool GenerateNavigationGraph { get; set; } = false;

        [Description("Adjust this if NPCs with custom scale are \"flying\"")]
        public float NpcSizePositionMultiplier { get; set; } = 1.3f;

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

        //Frequencies
        [Description("Update frequencies")]
        public float AIIdleUpdateFrequency { get; set; } = 0.3f;

        public float MovementUpdateFrequency { get; set; } = 0.1f;
        public float NavUpdateFrequency { get; set; } = 0.1f;

        [Description("Mappings listed there will be loaded on round start")]
        public List<string> InitialMappings { get; set; } = new List<string>();

        public static string NPCs_root_path = Path.Combine(Paths.Configs, "npcs");

        public static string NPCs_mappings_path = Path.Combine(NPCs_root_path, "mappings");

        public static string NPCs_nav_mappings_path = Path.Combine(NPCs_root_path, "nav_mappings.yml");

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
role: Scientist
health: -1
scale: [1, 1, 1]
item_held: GunLogicer
root_node: default_node.yml
god_mode: true
is_exclusive: true
events: []
ai_enabled: false
ai: []
";

        public static string DefaultNavMappings =
@"---
{}
";
    }
}