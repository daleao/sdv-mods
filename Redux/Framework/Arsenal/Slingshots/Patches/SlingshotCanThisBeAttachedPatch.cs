﻿namespace DaLion.Redux.Framework.Arsenal.Slingshots.Patches;

#region using directives

using HarmonyLib;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class SlingshotCanThisBeAttachedPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="SlingshotCanThisBeAttachedPatch"/> class.</summary>
    internal SlingshotCanThisBeAttachedPatch()
    {
        this.Target = this.RequireMethod<Slingshot>(nameof(Slingshot.canThisBeAttached));
    }

    #region harmony patches

    /// <summary>Patch to allow equipping radioactive ore.</summary>
    [HarmonyPostfix]
    private static void SlingshotCanThisBeAttachedPostfix(ref bool __result, SObject? o)
    {
        __result = __result || o is { bigCraftable.Value: false, ParentSheetIndex: 909 };
    }

    #endregion harmony patches
}