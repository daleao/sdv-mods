﻿namespace DaLion.Stardew.Professions.Framework.Patches.Integrations.LoveOfCooking;

#region using directives

using DaLion.Common;
using DaLion.Common.Attributes;
using DaLion.Common.Extensions.Reflection;
using DaLion.Common.Harmony;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Ultimates;

#endregion using directives

[UsedImplicitly, RequiresMod("blueberry.LoveOfCooking"), Deprecated]
internal sealed class ModEntryEvent_DrawRegenBarPatch : DaLion.Common.Harmony.HarmonyPatch
{
    /// <summary>Construct an instance.</summary>
    internal ModEntryEvent_DrawRegenBarPatch()
    {
        Target = "LoveOfCooking.ModEntry".ToType().RequireMethod("Event_DrawRegenBar");
    }

    #region harmony patches

    /// <summary>Patch to displace food bar.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? PropagatorPopExtraHeldMushroomsTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator, MethodBase original)
    {
        var helper = new ILHelper(original, instructions);

        /// Inject: if (ModEntry.PlayerState.RegisteredUltimate?.Hud.IsVisible) topOfBar.X -= 56f;
        /// Before: e.SpriteBatch.Draw( ... )

        var resumeExecution = ilGenerator.DefineLabel();
        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(OpCodes.Ldarg_2) // arg 2 = RenderingHudEventArgs e
                )
                .StripLabels(out var labels)
                .AddLabels(resumeExecution)
                .InsertWithLabels(
                    labels,
                    // check if RegisteredUltimate is null
                    new CodeInstruction(OpCodes.Call,
                        typeof(ModEntry).RequirePropertyGetter(nameof(ModEntry.Player))),
                    new CodeInstruction(OpCodes.Callvirt,
                        typeof(PlayerState).RequirePropertyGetter(nameof(PlayerState.RegisteredUltimate))),
                    new CodeInstruction(OpCodes.Brfalse_S, resumeExecution),
                    // check if RegisteredUltimate.Hud.IsVisible
                    new CodeInstruction(OpCodes.Call,
                        typeof(ModEntry).RequirePropertyGetter(nameof(ModEntry.Player))),
                    new CodeInstruction(OpCodes.Callvirt,
                        typeof(PlayerState).RequirePropertyGetter(nameof(PlayerState.RegisteredUltimate))),
                    new CodeInstruction(OpCodes.Callvirt,
                        typeof(Ultimate).RequirePropertyGetter(nameof(Ultimate.Hud))),
                    new CodeInstruction(OpCodes.Call,
                        typeof(UltimateHUD).RequirePropertyGetter(nameof(UltimateHUD.IsVisible))),
                    new CodeInstruction(OpCodes.Brfalse_S, resumeExecution),
                    // load and displace topOfBar.X
                    new CodeInstruction(OpCodes.Ldloca_S, helper.Locals[7]),
                    new CodeInstruction(OpCodes.Ldloc_S, helper.Locals[7]),
                    new CodeInstruction(OpCodes.Ldfld, typeof(Vector2).RequireField(nameof(Vector2.X))),
                    new CodeInstruction(OpCodes.Ldc_R4, 56f), // displace by 56 pixels
                    new CodeInstruction(OpCodes.Sub),
                    new CodeInstruction(OpCodes.Stfld, typeof(Vector2).RequireField(nameof(Vector2.X)))
                );
        }
        catch (Exception ex)
        {
            Log.E($"Failed while moving Love Of Cooking's food regen bar.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}