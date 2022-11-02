﻿namespace DaLion.Redux.Framework.Arsenal.Weapons.Patches;

#region using directives

using HarmonyLib;
using StardewValley.Tools;
using HarmonyPatch = Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class AmethystEnchantmentUnapplyToPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="AmethystEnchantmentUnapplyToPatch"/> class.</summary>
    internal AmethystEnchantmentUnapplyToPatch()
    {
        this.Target = this.RequireMethod<AmethystEnchantment>("_UnapplyTo");
    }

    #region harmony patches

    /// <summary>Rebalances Amethyst enchant.</summary>
    [HarmonyPrefix]
    private static bool AmethystEnchantmentUnapplyToPrefix(AmethystEnchantment __instance, Item item)
    {
        if (item is not MeleeWeapon weapon || !ModEntry.Config.Arsenal.RebalancedForges || !ModEntry.Config.Arsenal.OverhauledKnockback)
        {
            return true; // run original logic
        }

        weapon.knockback.Value -= __instance.GetLevel() * 0.1f;
        return false; // don't run original logic
    }

    #endregion harmony patches
}