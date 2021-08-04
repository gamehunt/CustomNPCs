using Exiled.API.Features;
using NPCS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

namespace NPCS.Talking
{
    public class TalkNode
    {
        private class TalkNodeSerializationInfo
        {
            public string Description { get; set; }
            public string Reply { get; set; }
            public List<NpcNodeWithArgsSerializationInfo> Conditions { get; set; }
            public List<NpcNodeWithArgsSerializationInfo> Actions { get; set; }

            [YamlMember(Alias = "next_nodes")]
            public string[] NextNodes { get; set; }
        }

        public TalkNode(string path)
        {
            Log.Debug($"Parsing node {path}", Plugin.Instance.Config.VerboseOutput);
            NodeFile = path;
            try
            {
                var input = new StringReader(File.ReadAllText(path));
                var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    // Workaround to remove YamlAttributesTypeInspector
                                    .WithTypeInspector(inner => inner, s => s.InsteadOf<YamlAttributesTypeInspector>())
                                    .WithTypeInspector(
                                        inner => new YamlAttributesTypeInspector(inner),
                                        s => s.Before<NamingConventionTypeInspector>()
                                    )
                                    .Build();

                TalkNodeSerializationInfo raw_node = deserializer.Deserialize<TalkNodeSerializationInfo>(input);

                Desc = raw_node.Description;
                Reply = raw_node.Reply;

                //Parse conditions
                //Format:
                //------------
                //conditions:
                // - token: SomeToken
                //   args:
                //    some_arg: some_value
                //    some_arg1: some_value1
                foreach (NpcNodeWithArgsSerializationInfo info in raw_node.Conditions)
                {
                    NodeCondition cond = NodeCondition.GetFromToken(info.Token);
                    if (cond != null)
                    {
                        Log.Debug($"Recognized token: {cond.Name}", Plugin.Instance.Config.VerboseOutput);
                        Conditions.Add(cond, info.Args);
                    }
                    else
                    {
                        Log.Error($"Failed to parse condition: {info.Token} (invalid token)");
                    }
                }

                //Parse actions
                //Format:
                //------------
                //actions:
                // - token: SomeToken
                //   args:
                //    some_arg: some_value
                //    some_arg1: some_value1
                foreach (NpcNodeWithArgsSerializationInfo info in raw_node.Actions)
                {
                    NodeAction cond = NodeAction.GetFromToken(info.Token);
                    if (cond != null)
                    {
                        Log.Debug($"Recognized token: {cond.Name}", Plugin.Instance.Config.VerboseOutput);
                        Actions.Add(cond, info.Args);
                    }
                    else
                    {
                        Log.Error($"Failed to parse action: {info.Token} (invalid token)");
                    }
                }

                //Parse next nodes
                //Format:
                //------------
                //next_nodes:
                // - /relative/path/to/node
                Log.Debug("Parsing next nodes...", Plugin.Instance.Config.VerboseOutput);
                foreach (string item in raw_node.NextNodes)
                {
                    NextNodes.Add(TalkNode.FromFile(Path.Combine(Config.DialogNodesDirectory, item)));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to parse node {path}! {e}");
                this.Desc = "<ERROR>";
                this.Reply = "<ERROR>";
            }
        }

        public List<TalkNode> NextNodes { get; set; } = new List<TalkNode>();

        public string Reply { get; set; } = "<Empty>";
        public string Desc { get; set; } = "<Empty Node>";

        public string NodeFile { get; set; } = "";

        public Dictionary<NodeCondition, Dictionary<string, string>> Conditions { get; set; } = new Dictionary<NodeCondition, Dictionary<string, string>>();
        public Dictionary<NodeAction, Dictionary<string, string>> Actions { get; set; } = new Dictionary<NodeAction, Dictionary<string, string>>();

        public static TalkNode FromFile(string file)
        {
            return new TalkNode(file);
        }

        public bool Send(string npc_name, Player p)
        {
            p.SendConsoleMessage("[" + npc_name + "]: " + Reply, "yellow");
            if (NextNodes.Count > 0)
            {
                string answs = "Answers: \n\n";
                int num = 0;
                foreach (TalkNode next in NextNodes)
                {
                    bool flag = true;
                    foreach (NodeCondition cond in next.Conditions.Keys)
                    {
                        try
                        {
                            if (!cond.Check(p, next.Conditions[cond]))
                            {
                                flag = false;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            flag = false;
                            Log.Error($"Exception during {cond.Name} condition check: {e}");
                            break;
                        }
                    }
                    if (flag)
                    {
                        answs += $"[{num}] {next.Desc}\n\n";
                        num++;
                    }
                }
                if (num != 0)
                {
                    p.SendConsoleMessage(answs, "yellow");
                    return false;
                }
            }
            return true;
        }
    }
}