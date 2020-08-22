﻿using Exiled.API.Features;
using System.Collections.Generic;
using UnityEngine;

namespace NPCS.Navigation
{
    internal class NavigationNode : MonoBehaviour
    {
        public int Priority { get; set; } = 100;

        public int Order { get; set; } = 0;

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
            foreach(NavigationNode navnode in AllNodes)
            {
                Object.Destroy(navnode);
            }
        }

        private void Awake()
        {
            Log.Debug($"Node created",Plugin.Instance.Config.VerboseOutput);
            AllNodes.Add(this);
        }

        private void OnDestroy()
        {
            Log.Debug("Node destroyed", Plugin.Instance.Config.VerboseOutput);
            AllNodes.Remove(this);
        }
    }
}