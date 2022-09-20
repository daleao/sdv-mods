﻿namespace DaLion.Stardew.Slingshots.Framework.Patches;

#region using directives

using Common;
using Common.Extensions.Reflection;
using Common.Harmony;
using HarmonyLib;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#endregion using directives

[UsedImplicitly]
internal sealed class ToolAddEnchantmentPatch : Common.Harmony.HarmonyPatch
{
    /// <summary>Construct an instance.</summary>
    internal ToolAddEnchantmentPatch()
    {
        Target = RequireMethod<Tool>(nameof(Tool.AddEnchantment));
    }

    #region harmony patches

    /// <summary>Allow Slingshot forges.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? ToolAddEnchantmentTranspiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator, MethodBase original)
    {
        var helper = new ILHelper(original, instructions);

        /// From: if (this is MeleeWeapon ...
        /// To: if (this is MeleeWeapon || this is Slingshot && ModEntry.Config.EnableSlingshotForges ...

        var isWeapon = generator.DefineLabel();
        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(OpCodes.Isinst)
                )
                .Advance()
                .GetOperand(out var resumeExecution)
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Brtrue_S, isWeapon),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Isinst, typeof(Slingshot)),
                    new CodeInstruction(OpCodes.Brfalse, resumeExecution),
                    new CodeInstruction(OpCodes.Call, typeof(ModEntry).RequirePropertyGetter(nameof(ModEntry.Config))),
                    new CodeInstruction(OpCodes.Call,
                        typeof(ModConfig).RequirePropertyGetter(nameof(ModConfig.EnableSlingshotForges)))
                )
                .Advance()
                .AddLabels(isWeapon);
        }
        catch (Exception ex)
        {
            Log.E($"Failed allowing add forges to slingshots.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}