﻿namespace DaLion.Redux.Framework.Professions.Patches.Fishing;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Redux.Framework.Professions.Extensions;
using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley.Locations;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class ForestGetFishPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="ForestGetFishPatch"/> class.</summary>
    internal ForestGetFishPatch()
    {
        this.Target = this.RequireMethod<Forest>(nameof(Forest.getFish));
    }

    #region harmony patches

    /// <summary>Patch for prestiged Angler to recatch Glacierfish.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? ForestGetFishTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: if (!who.fishCaught.ContainsKey(<legendary_fish_id>)) ...
        // To: if (!who.fishCaught.ContainsKey(<legendary_fish_id>) || !who.HasPrestigedProfession("Angler") ...
        try
        {
            var checkSeason = generator.DefineLabel();
            helper
                .FindFirst(new CodeInstruction(OpCodes.Ldc_I4, Constants.GlacierfishIndex))
                .AdvanceUntil(new CodeInstruction(OpCodes.Brtrue_S))
                .GetOperand(out var skipLegendary)
                .ReplaceInstructionWith(new CodeInstruction(OpCodes.Brfalse_S, checkSeason))
                .Advance()
                .AddLabels(checkSeason)
                .InsertInstructions(new CodeInstruction(OpCodes.Ldarg_S, (byte)4)) // arg 4 = Farmer who
                .InsertProfessionCheck(Profession.Angler.Value + 100, forLocalPlayer: false)
                .InsertInstructions(new CodeInstruction(OpCodes.Brfalse_S, skipLegendary));
        }
        catch (Exception ex)
        {
            Log.E($"Failed while adding prestiged Angler legendary fish recatch.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}