﻿namespace DaLion.Overhaul.Modules.Arsenal.Patchers.Crafting;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Shared.Extensions.Reflection;
using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

#endregion using directives

[UsedImplicitly]
internal sealed class ItemGrabMenuReceiveLeftClickPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="ItemGrabMenuReceiveLeftClickPatcher"/> class.</summary>
    internal ItemGrabMenuReceiveLeftClickPatcher()
    {
        this.Target = this.RequireMethod<ItemGrabMenu>(nameof(ItemGrabMenu.receiveLeftClick));
    }

    #region harmony patches

    /// <summary>Make Dwarvish Blueprint go *poof*.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? ItemGrabMenuReceiveLeftClickTranspiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
    {
        var helper = new ILHelper(original, instructions);

        try
        {
            var foundLostBook = generator.DefineLabel();
            var dwarvenBlueprintIndex = generator.DeclareLocal(typeof(int?));
            helper
                .Match(new[] { new CodeInstruction(OpCodes.Ldc_I4_S, 102) })
                .Match(new[] { new CodeInstruction(OpCodes.Brfalse_S) })
                .GetOperand(out var foundNothing)
                .ReplaceWith(new CodeInstruction(OpCodes.Brtrue_S, foundLostBook))
                .Move()
                .AddLabels(foundLostBook)
                .Insert(
                    new[]
                    {
                        new CodeInstruction(
                            OpCodes.Call,
                            typeof(Globals).RequirePropertyGetter(nameof(Globals.DwarvishBlueprintIndex))),
                        new CodeInstruction(OpCodes.Stloc_S, dwarvenBlueprintIndex),
                        new CodeInstruction(OpCodes.Ldloca_S, dwarvenBlueprintIndex),
                        new CodeInstruction(OpCodes.Call, typeof(int?).RequirePropertyGetter(nameof(Nullable<int>.HasValue))),
                        new CodeInstruction(OpCodes.Brfalse_S, foundNothing), new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(
                            OpCodes.Ldfld,
                            typeof(ItemGrabMenu).RequireField(nameof(ItemGrabMenu.heldItem))),
                        new CodeInstruction(OpCodes.Isinst, typeof(SObject)), new CodeInstruction(
                            OpCodes.Call,
                            typeof(Globals).RequirePropertyGetter(nameof(Globals.DwarvishBlueprintIndex))),
                        new CodeInstruction(OpCodes.Stloc_S, dwarvenBlueprintIndex),
                        new CodeInstruction(OpCodes.Ldloca_S, dwarvenBlueprintIndex),
                        new CodeInstruction(OpCodes.Call, typeof(int?).RequirePropertyGetter(nameof(Nullable<int>.Value))),
                        new CodeInstruction(
                            OpCodes.Call,
                            typeof(Utility).RequireMethod(nameof(Utility.IsNormalObjectAtParentSheetIndex))),
                        new CodeInstruction(OpCodes.Brfalse_S, foundNothing),
                    })
                .Match(new[] { new CodeInstruction(OpCodes.Ldc_I4_S, 102) })
                .ReplaceWith(
                    new CodeInstruction(
                        OpCodes.Callvirt,
                        typeof(Item).RequirePropertyGetter(nameof(Item.ParentSheetIndex))))
                .Insert(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(
                            OpCodes.Ldfld,
                            typeof(ItemGrabMenu).RequireField(nameof(ItemGrabMenu.heldItem))),
                    });
        }
        catch (Exception ex)
        {
            Log.E($"Failed injecting blueprint poof in grab menu.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches
}
