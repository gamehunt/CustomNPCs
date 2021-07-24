using Exiled.API.Extensions;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NPCS.Utils
{
    public class SerializableVector2
    {
        public SerializableVector2()
        {
        }

        public SerializableVector2(Vector2 vec)
        {
            x = vec.x;
            y = vec.y;
        }

        public float x { get; set; }
        public float y { get; set; }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    public class SerializableVector3
    {
        public SerializableVector3()
        {
        }

        public SerializableVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    public class NpcNodeSerializationInfo
    {
        public string Token { get; set; }
    }

    public class NpcEventSerializationInfo : NpcNodeSerializationInfo
    {
        public List<NpcNodeWithArgsSerializationInfo> Actions { get; set; }
    }

    public class NpcNodeWithArgsSerializationInfo : NpcNodeSerializationInfo
    {
        public Dictionary<string, string> Args { get; set; }
    }

    public enum AIMode
    {
        Legacy,
        Python
    }

    public class NpcSerializationInfo
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public RoleType Role { get; set; }
        public float[] Scale { get; set; }

        [YamlMember(Alias = "item_held")]
        public ItemType ItemHeld { get; set; }

        public ItemType[] Inventory { get; set; }

        [YamlMember(Alias = "root_node")]
        public string RootNode { get; set; }

        [YamlMember(Alias = "god_mode")]
        public bool GodMode { get; set; }

        [YamlMember(Alias = "is_exclusive")]
        public bool IsExclusive { get; set; }

        [YamlMember(Alias = "affect_summary")]
        public bool AffectSummary { get; set; }

        public NpcEventSerializationInfo[] Events { get; set; }

        [YamlMember(Alias = "ai_enabled")]
        public bool AiEnabled { get; set; }

        [YamlMember(Alias = "ai_mode")]
        public AIMode AiMode { get; set; }

        [YamlMember(Alias = "ai_scripts")]
        public string[] AiScripts { get; set; }
    }

    public class Utils
    {
        public static bool CompareWithType(string type, float a, float b)
        {
            switch (type)
            {
                case "equals":
                    return a.Equals(b);

                case "greater":
                    return a > b;

                case "less":
                    return a < b;

                case "greater_or_equals":
                    return a >= b;

                case "less_or_equals":
                    return a <= b;

                case "not_equals":
                    return !a.Equals(b);

                default:
                    return false;
            }
        }

        public static bool CheckItemType(string type, ItemType item)
        {
            switch (type)
            {
                case "keycard":
                    return item.IsKeycard();

                case "weapon":
                    return item.IsWeapon();

                default:
                    return item.ToString("g").Equals(type);
            }
        }
    }
}