﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.AnimalHusbandryMod;

#region using directives

using DaLion.Common;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using Extensions;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#endregion using directives

[UsedImplicitly, RequiresMod("DIGUS.ANIMALHUSBANDRYMOD")]
internal sealed class InseminationSyringeOverridesDoFunctionPatch : DaLion.Common.Harmony.HarmonyPatch
{
    /// <summary>Construct an instance.</summary>
    internal InseminationSyringeOverridesDoFunctionPatch()
    {
        Target = "AnimalHusbandryMod.tools.InseminationSyringeOverrides".ToType().RequireMethod("DoFunction");
    }

    #region harmony patches

    /// <summary>Patch to reduce gestation of animals inseminated by Breeder.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? InseminationSyringeOverridesDoFunctionTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new ILHelper(original, instructions);

        /// Injected: if (who.professions.Contains(<breeder_id>)) daysUntilBirth /= who.professions.Contains(<breeder_id> + 100) ? 3.0 : 2.0
        /// Before: PregnancyController.AddPregnancy(animal, daysUtillBirth);

        var daysUntilBirth = helper.Locals[5];
        var isNotBreeder = generator.DefineLabel();
        var isNotPrestiged = generator.DefineLabel();
        var resumeDivision = generator.DefineLabel();
        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_S, daysUntilBirth),
                    new CodeInstruction(OpCodes.Call)
                )
                .StripLabels(out var labels)
                .AddLabels(isNotBreeder)
                .InsertWithLabels(
                    labels,
                    new CodeInstruction(OpCodes.Ldarg_S, (byte)5) // arg 5 = Farmer who
                )
                .InsertProfessionCheck(Profession.Breeder.Value, forLocalPlayer: false)
                .Insert(
                    new CodeInstruction(OpCodes.Brfalse_S, isNotBreeder),
                    new CodeInstruction(OpCodes.Ldloc_S, daysUntilBirth),
                    new CodeInstruction(OpCodes.Conv_R8),
                    new CodeInstruction(OpCodes.Ldarg_S, (byte)5)
                )
                .InsertProfessionCheck(Profession.Breeder.Value + 100, forLocalPlayer: false)
                .Insert(
                    new CodeInstruction(OpCodes.Brfalse_S, isNotPrestiged),
                    new CodeInstruction(OpCodes.Ldc_R8, 3.0),
                    new CodeInstruction(OpCodes.Br_S, resumeDivision)
                )
                .InsertWithLabels(
                    new[] { isNotPrestiged },
                    new CodeInstruction(OpCodes.Ldc_R8, 2.0)
                )
                .InsertWithLabels(
                    new[] { resumeDivision },
                    new CodeInstruction(OpCodes.Div),
                    new CodeInstruction(OpCodes.Call,
                        typeof(Math).RequireMethod(nameof(Math.Round), new[] { typeof(double) })),
                    new CodeInstruction(OpCodes.Conv_I4),
                    new CodeInstruction(OpCodes.Stloc_S, daysUntilBirth)
                );
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching inseminated pregnancy time for Breeder.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}