using System;
using System.Collections.Generic;

namespace NPCS
{
    internal class Utils
    {
        public static IEnumerator<float> CallOnUnlock(Action act, Npc locked)
        {
            while (locked.IsActionLocked)
            {
                yield return 0f;
            }
            act.Invoke();
        }
    }
}