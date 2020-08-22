using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.Navigation
{
    internal class NavigationNode : MonoBehaviour
    {
        public int Priority { get; set; } = 100;

        public int Order { get; set; } = 0;

        public Vector3 Position
        {
            get
            {
                return gameObject.transform.position;
            }
            set
            {
                gameObject.transform.position = value;
            }
        }

        public string Name { get; private set; } = "DefaultNavNode";

        private List<NavigationNode> linked_nodes = new List<NavigationNode>();

        public static Dictionary<string,NavigationNode> AllNodes = new Dictionary<string, NavigationNode>();

        public static void Create(Vector3 pos, string name = "DefaultNavNode")
        {
            GameObject go = new GameObject();
            NavigationNode node = go.AddComponent<NavigationNode>();
            node.Name = name;
            Log.Debug($"Node created", Plugin.Instance.Config.VerboseOutput);
            AllNodes.Add(node.Name, node);
            go.transform.position = pos;
        }

        public static void Clear()
        {
            List<NavigationNode> nodes = AllNodes.Values.ToList();
            foreach (NavigationNode navnode in nodes)
            {
                Object.Destroy(navnode);
            }

        }

        public static NavigationNode Get(string name)
        {
            try
            {
                return AllNodes[name];
            }
            catch
            {
                return null;
            }
        }

        private void OnDestroy()
        {
            Log.Debug("Node destroyed", Plugin.Instance.Config.VerboseOutput);
            AllNodes.Remove(this.Name);
        }
    }
}