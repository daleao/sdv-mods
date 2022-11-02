﻿namespace DaLion.Redux.Framework.Arsenal.Slingshots.Patches;

#region using directives

using HarmonyLib;
using StardewValley.Tools;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class BaseWeaponEnchantmentCanApplyToPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="BaseWeaponEnchantmentCanApplyToPatch"/> class.</summary>
    internal BaseWeaponEnchantmentCanApplyToPatch()
    {
        this.Target = this.RequireMethod<BaseWeaponEnchantment>("CanApplyTo");
    }

    #region harmony patches

    /// <summary>Allow Slingshot forges.</summary>
    [HarmonyPostfix]
    private static void BaseWeaponEnchantmentCanApplyToPostfix(
        BaseWeaponEnchantment __instance, ref bool __result, Item item)
    {
        if (item is not Slingshot || __instance.IsSecondaryEnchantment())
        {
            return;
        }

        __result = __instance.IsForge() && ModEntry.Config.Arsenal.Slingshots.AllowForges;
    }

    #endregion harmony patches
}