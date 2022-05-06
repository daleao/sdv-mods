﻿namespace DaLion.Common.Extensions.Reflection;

#region using directives

using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

#endregion using directives

/// <summary>Extensions for the <see cref="MethodInfo"/> class.</summary>
public static class MethodInfoExtensions
{
    /// <summary>Construct a <see cref="HarmonyMethod" /> instance from a <see cref="MethodInfo" /> object.</summary>
    /// <returns>
    ///     Returns a new <see cref="HarmonyMethod" /> instance if <paramref name="method" /> is not null, or <c>null</c>
    ///     otherwise.
    /// </returns>
    [CanBeNull]
    public static HarmonyMethod ToHarmonyMethod(this MethodInfo method)
    {
        return method is null ? null : new HarmonyMethod(method);
    }
}