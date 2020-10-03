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

        [Description("Should PlayerList contain NPCs?")]
        public bool DisplayNpcInPlayerList { get; set; } = false;

        [Description("Should RA contain NPCs?")]
        public bool DisplayNpcInRemoteAdmin { get; set; } = false;

        [Description("If false NPCs will be cleaned if they are alone on server")]
        public bool AllowAloneNpcs { get; set; } = true;

        [Description("Maximum distance between NPC and follow target. If it's reached NPC will tp to target")]
        public float MaxFollowDistance { get; set; } = 15f;

        [Description("Enable this if you will use nav system (not neccessary for following)")]
        public bool GenerateNavigationGraph { get; set; } = false;

        [Description("Ajust this if NPCs with custom scale are \"flying\"")]
        public float NpcSizePositionMultiplier { get; set; } = 1.27f;

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
role: 6
health: -1
scale: [1, 1, 1]
item_held: 24
root_node: default_node.yml
god_mode: true
is_exclusive: true
events: []
ai_enabled: false
ai: []
";

        public static string DefaultNavMappings =
@"---
HCZ_Room3ar:
- Relative:
    x: 9.64225769
    y: 1.33001709
    z: 0.171562195
  RoomRotation: 180
- Relative:
    x: 4.34938049
    y: 1.33001709
    z: -0.0225715637
  RoomRotation: 180
- Relative:
    x: -0.253387451
    y: 1.33001709
    z: 5.84685898
  RoomRotation: 180
- Relative:
    x: -0.0796127319
    y: 1.33001709
    z: 9.6912117
  RoomRotation: 180
- Relative:
    x: -0.0617141724
    y: 1.32995605
    z: 9.70713806
  RoomRotation: 90
- Relative:
    x: 0.0109405518
    y: 1.33001709
    z: 5.05988312
  RoomRotation: 90
- Relative:
    x: -5.83143616
    y: 1.32995605
    z: -0.0288848877
  RoomRotation: 90
- Relative:
    x: -9.57893372
    y: 1.32995605
    z: 0.0152130127
  RoomRotation: 90
- Relative:
    x: 5.3494873
    y: 1.33001709
    z: -0.121322632
  RoomRotation: 90
- Relative:
    x: 9.65637207
    y: 1.33001709
    z: -0.0269927979
  RoomRotation: 90
- Relative:
    x: 0.119373322
    y: 1.33001709
    z: 9.63119507
  RoomRotation: 90
- Relative:
    x: -0.00559234619
    y: 1.33001709
    z: 4.79691315
  RoomRotation: 90
- Relative:
    x: -6.2363739
    y: 1.33001709
    z: -0.0487976074
  RoomRotation: 90
- Relative:
    x: -9.66663742
    y: 1.33001709
    z: -0.085067749
  RoomRotation: 90
- Relative:
    x: -5.23825455
    y: 1.33001709
    z: 0.0444259644
  RoomRotation: 90
- Relative:
    x: 0.132129669
    y: 1.33001709
    z: 5.01822662
  RoomRotation: 90
- Relative:
    x: 5.83706665
    y: 1.32995605
    z: -0.0788116455
  RoomRotation: 90
- Relative:
    x: 9.69130325
    y: 1.33001709
    z: -0.0573120117
  RoomRotation: 90
HCZ_Testroom:
- Relative:
    x: -0.368598938
    y: 1.33001709
    z: -9.59996796
  RoomRotation: 90
- Relative:
    x: 8.1031723
    y: 1.33001709
    z: -8.9469223
  RoomRotation: 90
- Relative:
    x: 9.27785492
    y: 1.33001709
    z: -6.43737793
  RoomRotation: 90
- Relative:
    x: 8.72302246
    y: 1.33001709
    z: 8.34698486
  RoomRotation: 90
- Relative:
    x: 6.74073792
    y: 1.33001709
    z: 9.22762299
  RoomRotation: 90
- Relative:
    x: -0.293174744
    y: 1.32995605
    z: 9.68888092
  RoomRotation: 90
HCZ_Servers:
- Relative:
    x: 8.13702393
    y: 1.32983398
    z: -0.0022354126
  RoomRotation: 0
- Relative:
    x: 8.40721893
    y: 1.32977295
    z: -8.30627441
  RoomRotation: 0
- Relative:
    x: -6.38092804
    y: -6.59008789
    z: -8.28289795
  RoomRotation: 0
- Relative:
    x: -6.2484436
    y: -6.59008789
    z: -0.82875061
  RoomRotation: 0
- Relative:
    x: 3.9041214
    y: -6.59008789
    z: -0.542068481
  RoomRotation: 0
- Relative:
    x: 4.09844971
    y: -6.59014893
    z: -3.23969269
  RoomRotation: 0
- Relative:
    x: -8.25559998
    y: 1.3293457
    z: -3.27078247
  RoomRotation: 0
- Relative:
    x: -8.32525635
    y: 1.32977295
    z: -0.355278015
  RoomRotation: 0
";
    }
}