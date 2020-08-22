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

        public string Name { get; set; } = "DefaultNavNode";

        private List<NavigationNode> linked_nodes = new List<NavigationNode>();

        public static List<NavigationNode> AllNodes = new List<NavigationNode>();

        public static void Create(Vector3 pos, string name = "DefaultNavNode")
        {
            GameObject go = new GameObject();
            NavigationNode node = go.AddComponent<NavigationNode>();
            node.Name = name;
            go.transform.position = pos;
        }

        public static void Clear()
        {
            foreach (NavigationNode navnode in AllNodes)
            {
                Object.Destroy(navnode);
            }
        }

        public static NavigationNode Get(string name)
        {
            return AllNodes.Where(n => n.Name == name).FirstOrDefault();
        }

        private void Awake()
        {
            Log.Debug($"Node created", Plugin.Instance.Config.VerboseOutput);
            AllNodes.Add(this);
        }

        private void OnDestroy()
        {
            Log.Debug("Node destroyed", Plugin.Instance.Config.VerboseOutput);
            AllNodes.Remove(this);
        }
    }
}