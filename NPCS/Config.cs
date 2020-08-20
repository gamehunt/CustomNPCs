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

        [Description("Should PlayerList contains NPCs?")]
        public bool DisplayNPCInPlayerList { get; set; } = false;

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
";
    }
}