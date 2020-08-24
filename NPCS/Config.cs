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
item_held: 24
root_node: default_node.yml
god_mode: true
is_exclusive: true
events: []
";
    }
}