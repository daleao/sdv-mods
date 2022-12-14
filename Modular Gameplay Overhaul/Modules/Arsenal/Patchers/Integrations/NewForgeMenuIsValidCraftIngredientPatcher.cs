namespace DaLion.Overhaul.Modules.Arsenal.Patchers.Integrations;

#region using directives

using DaLion.Shared.Attributes;
using DaLion.Shared.Harmony;
using HarmonyLib;
using SpaceCore.Interface;

#endregion using directives

[UsedImplicitly]
[RequiresMod("spacechase0.SpaceCore")]
internal sealed class NewForgeMenuIsValidCraftIngredientPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="NewForgeMenuIsValidCraftIngredientPatcher"/> class.</summary>
    internal NewForgeMenuIsValidCraftIngredientPatcher()
    {
        this.Target = this.RequireMethod<NewForgeMenu>(nameof(NewForgeMenu.IsValidCraftIngredient));
    }

    #region harmony patches

    /// <summary>Allow forging with Hero Soul.</summary>
    [HarmonyPostfix]
    private static void NewForgeMenuIsValidCraftIngredientPostfix(ref bool __result, Item item)
    {
        if (Globals.HeroSoulIndex.HasValue && item.ParentSheetIndex == Globals.HeroSoulIndex.Value)
        {
            __result = true;
        }
    }

    #endregion harmony patches
}
