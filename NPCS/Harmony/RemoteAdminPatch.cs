using HarmonyLib;
using NorthwoodLib.Pools;
using RemoteAdmin;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace NPCS.Harmony
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class RemoteAdminPatch
    {
        //My first transpiler, yay!
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            //int start_index = 10133;
            //int continue_index = 10256;
            int name_index = 10180;

            LocalBuilder go = generator.DeclareLocal(typeof(GameObject));

            var continueLabel = generator.DefineLabel();
            var skipLabel = generator.DefineLabel();
            var skipStrLabel = generator.DefineLabel();
            //newInstructions[continue_index].labels.Add(continueLabel);

            // if (Npc.Dictionary.ContainsKey(gameObject4))
            //    continue
            /*newInstructions.InsertRange(start_index, new[]
            {
                new CodeInstruction(OpCodes.Stloc_S, go.LocalIndex),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Plugin), nameof(Plugin.Instance))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Plugin), nameof(Plugin.Config))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Config), nameof(Config.DisplayNpcInRemoteAdmin))),
                new CodeInstruction(OpCodes.Brtrue_S, skipLabel),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Npc), nameof(Npc.Dictionary))),
                new CodeInstruction(OpCodes.Ldloc_S, go.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<GameObject,Npc>),nameof(Dictionary<GameObject,Npc>.ContainsKey))),
                new CodeInstruction(OpCodes.Brtrue_S, continueLabel),
                new CodeInstruction(OpCodes.Ldloc_S, go.LocalIndex),
            });*/

            //newInstructions[start_index + 10].labels.Add(skipLabel);

            // if (Npc.Dictionary.ContainsKey(component.gameObject))
            //    text4 = "[NPC] "
            newInstructions.InsertRange(name_index /*+ 10*/, new[]
            {
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Npc), nameof(Npc.Dictionary))),
                new CodeInstruction(OpCodes.Ldloc_S, 122),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(ServerRoles), nameof(ServerRoles.gameObject))),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<GameObject,Npc>),nameof(Dictionary<GameObject,Npc>.ContainsKey))),
                new CodeInstruction(OpCodes.Brfalse_S, skipStrLabel),
                new CodeInstruction(OpCodes.Ldstr, "[NPC] "),
                new CodeInstruction(OpCodes.Stloc_S, 120),
            });

            newInstructions[name_index /*+ 10*/ + 7].labels.Add(skipStrLabel);

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}