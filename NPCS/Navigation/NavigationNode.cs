using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.Navigation
{
    public class NavigationNode : MonoBehaviour
    {
        public int Priority { get; set; } = 100;

        public Vector3 Position
        {
            get
            {
                return gameObject.transform.position;
            }
        }

        public class NavNodeSerializationInfo
        {
            public Utils.SerializableVector3 Relative { get; set; }
            public float RoomRotation { get; set; }
        }

        public string Room { get; set; }

        public NavNodeSerializationInfo SInfo { get; private set; }

        public string Name { get; private set; } = "DefaultNavNode";

        public HashSet<NavigationNode> LinkedNodes = new HashSet<NavigationNode>();

        public Door AttachedDoor { get; set; }

        public KeyValuePair<Lift.Elevator, Lift>? AttachedElevator { get; set; } = null;

        public static Dictionary<string, NavigationNode> AllNodes = new Dictionary<string, NavigationNode>();

        public static NavigationNode Create(Vector3 pos, string name = "DefaultNavNode", string room = "")
        {
            if (AllNodes.ContainsKey(name))
            {
                return null;
            }
            GameObject go = new GameObject();
            NavigationNode node = go.AddComponent<NavigationNode>();
            node.Name = name;
            node.Room = room;
            Room r = Map.Rooms.Where(rm => rm.Name.Equals(room, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (r != null)
            {
                node.SInfo = new NavNodeSerializationInfo
                {
                    Relative = new Utils.SerializableVector3(pos - r.Position),
                    RoomRotation = r.Transform.localRotation.eulerAngles.y
                };
            }
            go.transform.position = pos;
            AllNodes.Add(node.Name, node);
            Log.Debug($"Node created: {name} at {pos}", Plugin.Instance.Config.VerboseOutput);
            return node;
        }

        public static NavigationNode Create(NavNodeSerializationInfo info, string name = "DefaultNavNode", string room = "")
        {
            if (AllNodes.ContainsKey(name))
            {
                return null;
            }
            GameObject go = new GameObject();
            NavigationNode node = go.AddComponent<NavigationNode>();
            node.Name = name;
            node.Room = room;
            node.SInfo = info;
            Room r = Map.Rooms.Where(rm => rm.Name.Equals(room, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (r != null)
            {
                go.transform.position = r.Position + Quaternion.Euler(0, r.Transform.localRotation.eulerAngles.y - info.RoomRotation, 0) * info.Relative.ToVector3();
            }
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
                UnityEngine.Object.Destroy(navnode);
            }
            AllNodes.Clear();
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
            Log.Debug($"Node destroyed: {Name}", Plugin.Instance.Config.VerboseOutput);
            AllNodes.Remove(Name);
        }
    }
}