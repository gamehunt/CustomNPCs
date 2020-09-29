using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

namespace EndConditionsCompatModule.Harmony
{
    [HarmonyPatch(typeof(EndConditions.Handler), nameof(EndConditions.Handler.OnCheckRoundEnd))]
    internal class RoundEndCheckPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldloc_2);
            int continue_index = newInstructions.FindIndex(i => i.opcode == OpCodes.Leave_S) - 3;

            Log.Info($"Found: {index} {continue_index}");

            var continueLabel = generator.DefineLabel();
            newInstructions[continue_index].labels.Add(continueLabel);

            //if(player.IsNPC()){
            //   continue;
            //}

            newInstructions.InsertRange(index + 1, new[]
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(NPCS.Extensions), nameof(NPCS.Extensions.IsNPC))),
                new CodeInstruction(OpCodes.Brtrue_S,continueLabel),
                new CodeInstruction(OpCodes.Ldloc_2),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}