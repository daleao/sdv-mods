﻿namespace DaLion.Overhaul.Modules.Core.Patchers;

#region using directives

using System.Diagnostics;
using DaLion.Shared.Attributes;
using DaLion.Shared.Extensions.Reflection;
using DaLion.Shared.Harmony;
using HarmonyLib;

#endregion using directives

[UsedImplicitly]
[Debug]
internal sealed class DebugPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="DebugPatcher"/> class.</summary>
    internal DebugPatcher()
    {
    }

    #region harmony patches

    /// <summary>Placeholder patch for debugging.</summary>
    [HarmonyPrefix]
    internal static bool DebugPrefix()
    {
        var caller = new StackTrace().GetFrame(1)?.GetMethod()?.GetFullName();
        //Log.D($"{caller} prefix called!");
        return true;
    }

    /// <summary>Placeholder patch for debugging.</summary>
    [HarmonyPostfix]
    internal static void DebugPostfix()
    {
        var caller = new StackTrace().GetFrame(1)?.GetMethod()?.GetFullName();
        //Log.D($"{caller} postfix called!");
    }

    #endregion harmony patches
}
