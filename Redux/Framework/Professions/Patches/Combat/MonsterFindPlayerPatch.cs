﻿namespace DaLion.Redux.Framework.Professions.Patches.Combat;

#region using directives

using System.Linq;
using System.Reflection;
using DaLion.Redux.Framework.Professions.Extensions;
using DaLion.Redux.Framework.Professions.VirtualProperties;
using DaLion.Shared.Extensions.Stardew;
using HarmonyLib;
using StardewValley.Monsters;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class MonsterFindPlayerPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="MonsterFindPlayerPatch"/> class.</summary>
    internal MonsterFindPlayerPatch()
    {
        this.Target = this.RequireMethod<Monster>("findPlayer");
        this.Prefix!.priority = Priority.First;
    }

    #region harmony patches

    /// <summary>Patch to override monster aggro.</summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool MonsterFindPlayerPrefix(Monster __instance, ref Farmer? __result)
    {
        try
        {
            var location = Game1.currentLocation;
            Farmer? target = null;
            if (__instance is GreenSlime slime && slime.Get_Piper() is not null)
            {
                var aggroee = slime.GetClosestCharacter(location.characters.OfType<Monster>().Where(m => !m.IsSlime()));
                if (aggroee is not null)
                {
                    var fakeFarmer = slime.Get_FakeFarmer();
                    if (fakeFarmer is not null)
                    {
                        fakeFarmer.Position = aggroee.Position;
                        target = fakeFarmer;
                    }
                }
            }
            else
            {
                var taunter = __instance.Get_Taunter().Get(__instance.currentLocation);
                if (taunter is not null)
                {
                    var fakeFarmer = __instance.Get_FakeFarmer();
                    if (fakeFarmer is not null)
                    {
                        fakeFarmer.Position = taunter.Position;
                        target = fakeFarmer;
                    }
                }
            }

            __result = target ?? (Context.IsMultiplayer
                ? __instance.GetClosestFarmer(predicate: f => !f.Get_IsFake().Value && !f.IsInAmbush())
                : Game1.player);
            return false; // don't run original logic
        }
        catch (Exception ex)
        {
            Log.E($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}");
            return true; // default to original logic
        }
    }

    #endregion harmony patches
}