using HarmonyLib;
using PlayerRoles.Ragdolls;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mistaken.BetterSCP.SCP914;

[HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.ServerSpawnRagdoll))]
internal static class RagdollManagerPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Rent(instructions);

        int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldloc_0);

        var label = newInstructions[index].ExtractLabels()[0];
        var skipLabel = generator.DefineLabel();
        newInstructions[index].WithLabels(skipLabel);

        newInstructions.InsertRange(index, new CodeInstruction[]
        {
            new CodeInstruction(OpCodes.Ldarg_1).WithLabels(label),
            new(OpCodes.Call, AccessTools.Method(typeof(SCP914Handler), "OnRagdollSpawn")),
            new(OpCodes.Brtrue_S, skipLabel),
            new(OpCodes.Ldnull),
            new(OpCodes.Ret),
        });

        foreach (var instruction in newInstructions)
            yield return instruction;

        NorthwoodLib.Pools.ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}
