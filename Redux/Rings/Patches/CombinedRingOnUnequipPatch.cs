﻿namespace DaLion.Redux.Rings.Patches;

#region using directives

using DaLion.Redux.Rings.VirtualProperties;
using HarmonyLib;
using StardewValley.Objects;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class CombinedRingOnUnequipPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="CombinedRingOnUnequipPatch"/> class.</summary>
    internal CombinedRingOnUnequipPatch()
    {
        this.Target = this.RequireMethod<CombinedRing>(nameof(CombinedRing.onUnequip));
    }

    #region harmony patches

    /// <summary>Remove Infinity Band resonance effects.</summary>
    [HarmonyPostfix]
    private static void CombinedRingOnUnequipPostfix(CombinedRing __instance, Farmer who)
    {
        __instance.Get_Chord()?.Unapply(who.currentLocation, who);
    }

    #endregion harmony patches
}