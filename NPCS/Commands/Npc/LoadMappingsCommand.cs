using CommandSystem;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace NPCS.Commands.Npc
{
    internal class LoadMappingsCommand : ICommand
    {
        public string Command => "load_mappings";

        public string[] Aliases => new string[] { "load_mappings" };

        public string Description => "Loads npcs from file";

        private static IEnumerator<float> NPCMappingsLoadCoroutine(List<NPCS.Npc.NPCMappingInfo> infos)
        {
            foreach (NPCS.Npc.NPCMappingInfo info in infos)
            {
                Methods.LoadNPC(info);
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if(arguments.Count < 1) {
                response = "Path required";
                return false;
            }
            string path = Path.Combine(Config.MappingsDirectory, arguments.Array[0]);
            StreamReader sr;
            if (File.Exists(path))
            {
                sr = File.OpenText(path);
                var deserializer = new DeserializerBuilder().Build();
                List<NPCS.Npc.NPCMappingInfo> infos = deserializer.Deserialize<List<NPCS.Npc.NPCMappingInfo>>(sr);
                sr.Close();
                if (infos != null)
                {
                    Timing.RunCoroutine(NPCMappingsLoadCoroutine(infos));
                }
                else
                {
                    response = "Failed to load npc mappings: Format error!";
                    return false;
                }
            }
            else
            {
                response = "Failed to load npc mappings: File not exists!";
                return false;
            }
            response = "Loaded mappings";
            return true;
        }
    }
}