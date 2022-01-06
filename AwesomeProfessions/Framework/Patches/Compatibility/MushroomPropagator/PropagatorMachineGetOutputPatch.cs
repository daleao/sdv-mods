﻿using HarmonyLib;
using JetBrains.Annotations;
using StardewValley;
using TheLion.Stardew.Common.Harmony;
using TheLion.Stardew.Professions.Framework.Extensions;
using SObject = StardewValley.Object;

namespace TheLion.Stardew.Professions.Framework.Patches.Compatibility.MushroomPropagator;

[UsedImplicitly]
internal class PropagatorMachineGetOutput : BasePatch
{
    /// <summary>Construct an instance.</summary>
    internal PropagatorMachineGetOutput()
    {
        try
        {
            Original = "BlueberryMushroomAutomation.PropagatorMachine".ToType()
                .MethodNamed("GetOutput");
        }
        catch
        {
            // ignored
        }
    }

    #region harmony patches

    /// <summary>Patch for automated Propagator forage decrement.</summary>
    [HarmonyPostfix]
    private static void PropagatorMachineGetOutputPostfix(object __instance)
    {
        if (__instance is null || !ModEntry.Config.ShouldCountAutomatedHarvests) return;

        var entity = ModEntry.ModHelper.Reflection.GetProperty<SObject>(__instance, "Entity").GetValue();
        if (entity is null) return;

        var who = Game1.getFarmerMaybeOffline(entity.owner.Value) ?? Game1.MasterPlayer;
        if (!who.HasProfession("Ecologist")) return;

        if (who.IsLocalPlayer)
            ModData.Increment("ItemsForaged", -1);
        else
            ModData.Increment<uint>("ItemsForaged", who);
    }

    #endregion harmony patches
}