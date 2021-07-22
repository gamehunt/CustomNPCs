using Exiled.API.Features;
using FakePlayerAPI;

namespace NPCS
{
    public static class Extensions
    {
        public static bool IsNPC(this Player p)
        {
            return p.IsFakePlayer() && p.SessionVariables.ContainsKey("IsNPC");
        }

        public static Npc AsNPC(this Player p)
        {
            return p.IsNPC() ? (Npc)p.AsFakePlayer() : null;
        }

        public static bool IsClosed(this Lift lift, Lift.Elevator elev)
        {
            for (int i = 0; i < lift.elevators.Length; i++)
            {
                if (lift.elevators[i] == elev)
                {
                    if (i == 0)
                    {
                        return lift.NetworkstatusID == (int)Lift.Status.Down;
                    }
                    else
                    {
                        return lift.NetworkstatusID == (int)Lift.Status.Up;
                    }
                }
            }
            return lift.NetworkstatusID != (int)Lift.Status.Moving;
        }
    }
}