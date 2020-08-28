using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.Navigation
{
    internal class NavigationNode : MonoBehaviour
    {
        public int Priority { get; set; } = 100;

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

        public List<NavigationNode> LinkedNodes = new List<NavigationNode>();

        public Door AttachedDoor { get; set; }

        public static Dictionary<string, NavigationNode> AllNodes = new Dictionary<string, NavigationNode>();

        public static NavigationNode Create(Vector3 pos, string name = "DefaultNavNode")
        {
            if (AllNodes.ContainsKey(name))
            {
                return null;
            }
            GameObject go = new GameObject();
            NavigationNode node = go.AddComponent<NavigationNode>();
            node.Name = name;
            go.transform.position = pos;
            AllNodes.Add(node.Name, node);
            Log.Debug($"Node created: {name}", Plugin.Instance.Config.VerboseOutput);
            return node;
        }

        public static NavigationNode FromRoom(Room r)
        {
            try
            {
                return AllNodes[$"AUTO_Room_{r.Name}".Replace(' ', '_')];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
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