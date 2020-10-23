using Exiled.API.Features;
using NPCS.AI.TargetFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.AI
{
    internal class AIFindPlayerTarget : AITarget
    {
        public override string Name => "AIFindPlayerTarget";

        public override string[] RequiredArguments => new string[] { "range", "filter", "role_blacklist", "role_whitelist", "allow_self_select" };

        public override bool Check(Npc npc)
        {
            return true;
        }

        private readonly HashSet<RoleType> allowed_roles = new HashSet<RoleType>();
        private readonly HashSet<RoleType> disallowed_roles = new HashSet<RoleType>();
        private float range = 0f;
        private bool allow_self_select = false;

        public override float Process(Npc npc)
        {
            IsFinished = true;
            string target_filter = Arguments["filter"];

            foreach (Player p in Player.List.Where(pl => (pl != npc.NPCPlayer || allow_self_select) && (allowed_roles.Count == 0 || allowed_roles.Contains(pl.Role)) && (disallowed_roles.Count == 0 || !disallowed_roles.Contains(pl.Role))))
            {
                if (Vector3.Distance(p.Position, npc.NPCPlayer.Position) < range && !Physics.Linecast(npc.NPCPlayer.Position, p.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                {
                    bool res = true;
                    TargetFilter filter = TargetFilter.GetFromToken(target_filter);
                    if (filter != null)
                    {
                        res = filter.Check(npc, p);
                    }
                    if (res)
                    {
                        Log.Debug($"Selected target: {p.Nickname}", Plugin.Instance.Config.VerboseOutput);
                        npc.CurrentAIPlayerTarget = p;
                        return 0f;
                    }
                }
            }
            Log.Debug($"Selected null target", Plugin.Instance.Config.VerboseOutput);
            npc.CurrentAIPlayerTarget = null;
            return 0f;
        }

        protected override AITarget CreateInstance()
        {
            return new AIFindPlayerTarget();
        }

        public override void Construct()
        {
            range = float.Parse(Arguments["range"].Replace(".", ","));
            allow_self_select = bool.Parse(Arguments["allow_self_select"]);

            string[] raw_allowed_roles = Arguments["role_whitelist"].Split(',');
            foreach (string role in raw_allowed_roles)
            {
                if (role.Length != 0)
                {
                    RoleType erole = (RoleType)Enum.Parse(typeof(RoleType), role.Trim());
                    Log.Debug($"Added {erole:g} as allowed", Plugin.Instance.Config.VerboseOutput);
                    allowed_roles.Add(erole);
                }
            }

            string[] raw_blacklist_roles = Arguments["role_blacklist"].Split(',');
            foreach (string role in raw_blacklist_roles)
            {
                if (role.Length != 0)
                {
                    RoleType erole = (RoleType)Enum.Parse(typeof(RoleType), role.Trim());
                    Log.Debug($"Added {erole:g} as disallowed", Plugin.Instance.Config.VerboseOutput);
                    disallowed_roles.Add(erole);
                }
            }
        }
    }
}