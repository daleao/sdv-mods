﻿namespace DaLion.Overhaul.Modules.Rings.Patchers;

#region using directives

using DaLion.Shared.Harmony;
using HarmonyLib;
using Netcode;
using StardewValley.Objects;

#endregion using directives

[UsedImplicitly]
internal sealed class CombinedRingGetOneFromPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="CombinedRingGetOneFromPatcher"/> class.</summary>
    internal CombinedRingGetOneFromPatcher()
    {
        this.Target = this.RequireMethod<CombinedRing>(nameof(CombinedRing._GetOneFrom));
        this.Prefix!.priority = Priority.HigherThanNormal;
    }

    #region harmony patches

    /// <summary>Changes combined ring to Infinity Band when getting one.</summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.HigherThanNormal)]
    private static bool CombinedRingGetOneFromPrefix(CombinedRing __instance, Item source)
    {
        if (!Globals.InfinityBandIndex.HasValue || source.ParentSheetIndex != Globals.InfinityBandIndex)
        {
            return true; // run original logic
        }

        __instance.ParentSheetIndex = Globals.InfinityBandIndex.Value;
        ModHelper.Reflection.GetField<NetInt>(__instance, nameof(Ring.indexInTileSheet)).GetValue()
            .Set(Globals.InfinityBandIndex.Value);
        return true; // run original logic
    }

    #endregion harmony patches
}
