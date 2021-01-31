using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPCS.AI.Python
{
    public class NPCAIHelper
    {
        public NPCAIHelper(Npc parent)
        {
            npc = parent;
        }

        private Npc npc;

        private List<TargetFilters.TargetFilter> PrepareFilters(IronPython.Runtime.List filters)
        {
            List<TargetFilters.TargetFilter> fobjs = new List<TargetFilters.TargetFilter>();
            if (filters != null)
            {
                foreach (string flts in filters)
                {
                    TargetFilters.TargetFilter filtr = TargetFilters.TargetFilter.GetFromToken(flts);
                    if (filtr != null)
                    {
                        fobjs.Add(filtr);
                    }
                }
            }
            return fobjs;
        }

        public bool CheckPlayer(Player ply, IronPython.Runtime.List rfilters)
        {
            if (rfilters != null)
            {
                List<TargetFilters.TargetFilter> filters = PrepareFilters(rfilters);
                foreach (TargetFilters.TargetFilter filter in filters)
                {
                    if (!filter.Check(npc, ply))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public List<Player> GetNearestPlayers(float range, IronPython.Runtime.List filters = null, bool include_npcs = false)
        {
            List<Player> resulted = new List<Player>();
            List<TargetFilters.TargetFilter> fobjs = PrepareFilters(filters);
            foreach (Player p in Player.List.Where(pp => pp != npc.NPCPlayer && pp.IsAlive && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range))
            {
                foreach (TargetFilters.TargetFilter filter in fobjs)
                {
                    if (filter.Check(npc, p))
                    {
                        resulted.Add(p);
                    }
                }
            }
            return resulted;
        }

        public List<Player> GetNearestPlayers(float range, RoleType role, IronPython.Runtime.List filters = null, bool include_npcs = false)
        {
            List<Player> resulted = new List<Player>();
            List<TargetFilters.TargetFilter> fobjs = PrepareFilters(filters);
            foreach (Player p in Player.Get(role).Where(pp => pp != npc.NPCPlayer && pp.IsAlive && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range))
            {
                foreach (TargetFilters.TargetFilter filter in fobjs)
                {
                    if (filter.Check(npc, p))
                    {
                        resulted.Add(p);
                    }
                }
            }
            return resulted;
        }

        public List<Player> GetNearestPlayers(float range, RoleType[] roles, IronPython.Runtime.List filters = null, bool IsBlacklist = false, bool include_npcs = false)
        {
            List<Player> resulted = new List<Player>();
            List<TargetFilters.TargetFilter> fobjs = PrepareFilters(filters);
            IEnumerable<Player> raw_players;
            if (!IsBlacklist)
            {
                raw_players = Player.List.Where(pp => pp != npc.NPCPlayer && pp.IsAlive && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range && roles.Contains(pp.Role));
            }
            else
            {
                raw_players = Player.List.Where(pp => pp != npc.NPCPlayer && pp.IsAlive && (!pp.IsNPC() || include_npcs) && Vector3.Distance(pp.Position, npc.NPCPlayer.Position) <= range && !roles.Contains(pp.Role));
            }
            foreach (Player p in raw_players)
            {
                foreach (TargetFilters.TargetFilter filter in fobjs)
                {
                    if (filter.Check(npc, p))
                    {
                        resulted.Add(p);
                    }
                }
            }
            return resulted;
        }
    }
}