﻿namespace DaLion.Overhaul.Modules.Arsenal.Patchers.Infinity;

#region using directives

using System.Reflection;
using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley.Menus;

#endregion using directives

[UsedImplicitly]
internal sealed class ForgeMenuSpendRightItemPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="ForgeMenuSpendRightItemPatcher"/> class.</summary>
    internal ForgeMenuSpendRightItemPatcher()
    {
        this.Target = this.RequireMethod<ForgeMenu>(nameof(ForgeMenu.SpendRightItem));
    }

    #region harmony patches

    /// <summary>Prevent spending Hero Soul.</summary>
    [HarmonyPrefix]
    private static bool ForgeMenuSpendRightItemPrefix(ForgeMenu __instance)
    {
        try
        {
            return !Globals.HeroSoulIndex.HasValue || __instance.rightIngredientSpot.item?.ParentSheetIndex != Globals.HeroSoulIndex.Value;
        }
        catch (Exception ex)
        {
            Log.E($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}");
            return true; // default to original logic
        }
    }

    #endregion harmony patches
}
