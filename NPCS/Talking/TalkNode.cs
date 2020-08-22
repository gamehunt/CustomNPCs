using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;

using YamlDotNet.RepresentationModel;

namespace NPCS.Talking
{
    internal class TalkNode
    {
        public TalkNode(string path)
        {
            Log.Debug($"Parsing node {path}", Plugin.Instance.Config.VerboseOutput);
            NodeFile = path;
            try
            {
                var input = new StringReader(File.ReadAllText(path));

                var yaml = new YamlStream();
                yaml.Load(input);

                var mapping =
                    (YamlMappingNode)yaml.Documents[0].RootNode;

                Log.Debug("Parsing base info...", Plugin.Instance.Config.VerboseOutput);

                this.Desc = (string)mapping.Children[new YamlScalarNode("description")];
                this.Reply = (string)mapping.Children[new YamlScalarNode("reply")];

                //Parse conditions
                //Format:
                //------------
                //conditions:
                // - token: SomeToken
                //   args:
                //    some_arg: some_value
                //    some_arg1: some_value1

                Log.Debug("Parsing conditions...", Plugin.Instance.Config.VerboseOutput);
                var conditions = (YamlSequenceNode)mapping.Children[new YamlScalarNode("conditions")];
                Log.Debug($"{conditions.Children.Count} entries found", Plugin.Instance.Config.VerboseOutput);
                foreach (YamlMappingNode item in conditions)
                {
                    NodeCondition cond = NodeCondition.GetFromToken((string)item.Children[new YamlScalarNode("token")]);
                    if (cond != null)
                    {
                        Log.Debug($"Recognized token: {cond.Name}", Plugin.Instance.Config.VerboseOutput);
                        var yml_args = (YamlMappingNode)item.Children[new YamlScalarNode("args")];
                        Dictionary<string, string> arg_bindings = new Dictionary<string, string>();
                        foreach (YamlScalarNode arg in yml_args.Children.Keys)
                        {
                            arg_bindings.Add((string)arg, (string)yml_args.Children[arg]);
                        }
                        Conditions.Add(cond, arg_bindings);
                    }
                    else
                    {
                        Log.Error($"Failed to parse condition: {(string)item.Children[new YamlScalarNode("token")]} (invalid token)");
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

                Log.Debug("Parsing actions...", Plugin.Instance.Config.VerboseOutput);
                var actions = (YamlSequenceNode)mapping.Children[new YamlScalarNode("actions")];
                Log.Debug($"{actions.Children.Count} entries found", Plugin.Instance.Config.VerboseOutput);
                foreach (YamlMappingNode item in actions)
                {
                    NodeAction act = NodeAction.GetFromToken((string)item.Children[new YamlScalarNode("token")]);
                    if (act != null)
                    {
                        Log.Debug($"Recognized token: {act.Name}", Plugin.Instance.Config.VerboseOutput);
                        var yml_args = (YamlMappingNode)item.Children[new YamlScalarNode("args")];
                        Dictionary<string, string> arg_bindings = new Dictionary<string, string>();
                        foreach (YamlScalarNode arg in yml_args.Children.Keys)
                        {
                            arg_bindings.Add((string)arg.Value, (string)yml_args.Children[arg]);
                        }
                        Actions.Add(act, arg_bindings);
                    }
                    else
                    {
                        Log.Error($"Failed to parse action: {(string)item.Children[new YamlScalarNode("token")]} (invalid token)");
                    }
                }

                //Parse next nodes
                //Format:
                //------------
                //next_nodes:
                // - /relative/path/to/node
                Log.Debug("Parsing next nodes...", Plugin.Instance.Config.VerboseOutput);
                var next = (YamlSequenceNode)mapping.Children[new YamlScalarNode("next_nodes")];
                foreach (YamlScalarNode item in next)
                {
                    NextNodes.Add(TalkNode.FromFile(Path.Combine(Config.NPCs_nodes_path, (string)item.Value)));
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