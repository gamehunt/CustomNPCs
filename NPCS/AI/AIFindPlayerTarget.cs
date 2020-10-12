using Exiled.API.Features;
using NPCS.AI.TargetFilters;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NPCS.AI
{
    internal class AIFindPlayerTarget : AITarget
    {
        public override string Name => "AIFindPlayerTarget";

        private static readonly Scp939TargetFilter scp939_filter = new Scp939TargetFilter(); //TODO make registry for target filters

        public override bool Check(Npc npc)
        {
            return true;
        }

        private readonly HashSet<RoleType> allowed_roles = new HashSet<RoleType>();
        private readonly HashSet<RoleType> disallowed_roles = new HashSet<RoleType>();
        float range;

        public override float Process(Npc npc)
        {
            IsFinished = true;
            string target_filter = Arguments["filter"];

            foreach (Player p in Player.List.Where(pl => (allowed_roles.Count == 0 || allowed_roles.Contains(pl.Role)) && (disallowed_roles.Count == 0 || !disallowed_roles.Contains(pl.Role))))
            {
                if (Vector3.Distance(p.Position, npc.NPCPlayer.Position) < range && !Physics.Linecast(npc.NPCPlayer.Position, p.Position, npc.NPCPlayer.ReferenceHub.playerMovementSync.CollidableSurfaces))
                {
                    bool res = true;
                    if (target_filter == "scp939")
                    {
                        res = scp939_filter.Check(npc, p);
                    }
                    if (res)
                    {
                        npc.CurrentAIPlayerTarget = p;
                        return 0f;
                    }
                }
            }
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

            string[] raw_allowed_roles = Arguments["role_whitelist"].Split(',');
            foreach (string role in raw_allowed_roles)
            {
                allowed_roles.Add((RoleType)Enum.Parse(typeof(RoleType), role.Trim()));
            }

            string[] raw_blacklist_roles = Arguments["role_blacklist"].Split(',');
            foreach (string role in raw_blacklist_roles)
            {
                disallowed_roles.Add((RoleType)Enum.Parse(typeof(RoleType), role.Trim()));
            }
        }
    }
}