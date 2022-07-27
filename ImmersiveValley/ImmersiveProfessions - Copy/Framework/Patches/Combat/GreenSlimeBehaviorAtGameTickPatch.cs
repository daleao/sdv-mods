﻿namespace DaLion.Stardew.Professions.Framework.Patches.Combat;

#region using directives

using DaLion.Common.Attributes;
using DaLion.Common.ModData;
using HarmonyLib;
using JetBrains.Annotations;
using StardewValley;
using StardewValley.Monsters;

#endregion using directives

[UsedImplicitly, Deprecated]
internal sealed class GreenSlimeBehaviorAtGameTickPatch : DaLion.Common.Harmony.HarmonyPatch
{
    /// <summary>Construct an instance.</summary>
    internal GreenSlimeBehaviorAtGameTickPatch()
    {
        Target = RequireMethod<GreenSlime>(nameof(GreenSlime.behaviorAtGameTick));
    }

    #region harmony patches

    /// <summary>Patch to countdown jump timers.</summary>
    [HarmonyPostfix]
    private static void GreenSlimeBehaviorAtGameTickPostfix(GreenSlime __instance, ref int ___readyToJump)
    {
        var timeLeft = ModDataIO.Read<int>(__instance, "Jumping");
        if (timeLeft <= 0) return;

        timeLeft -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
        ModDataIO.Write(__instance, "Jumping", timeLeft <= 0 ? null : timeLeft.ToString());

        //if (!__instance.Player.HasProfession(Profession.Piper)) return;

        //___readyToJump = -1;
    }

    #endregion harmony patches
}