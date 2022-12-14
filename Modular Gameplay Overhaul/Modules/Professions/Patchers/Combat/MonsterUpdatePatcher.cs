namespace DaLion.Overhaul.Modules.Professions.Patchers.Combat;

#region using directives

using DaLion.Overhaul.Modules.Professions.VirtualProperties;
using DaLion.Shared.Harmony;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

#endregion using directives

[UsedImplicitly]
internal sealed class MonsterUpdatePatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="MonsterUpdatePatcher"/> class.</summary>
    internal MonsterUpdatePatcher()
    {
        this.Target =
            this.RequireMethod<Monster>(nameof(Monster.update), new[] { typeof(GameTime), typeof(GameLocation) });
        this.Prefix!.priority = Priority.First;
    }

    #region harmony patches

    /// <summary>Patch to implement slow.</summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool MonsterUpdatePrefix(Monster __instance, GameTime time)
    {
        var slowTimer = __instance.Get_SlowTimer();
        if (slowTimer.Value <= 0)
        {
            return true; // run original logic
        }

        slowTimer.Value -= time.ElapsedGameTime.Milliseconds;
        var slowIntensity = __instance.Get_SlowIntensity();
        __instance.startGlowing(Color.LimeGreen, false, 0.05f);
        return time.TotalGameTime.Ticks % slowIntensity.Value == 0; // conditionally run original logic
    }

    #endregion harmony patches
}
