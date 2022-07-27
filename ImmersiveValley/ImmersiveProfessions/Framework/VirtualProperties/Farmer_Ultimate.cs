﻿namespace DaLion.Stardew.Professions.Framework.VirtualProperties;

#region using directives

using Common.ModData;
using Netcode;
using StardewValley;
using System.Runtime.CompilerServices;
using Ultimates;

#endregion using directives

public static class Farmer_Ultimate
{
    internal class Holder
    {
        public Ultimate? ultimate;
        public readonly NetInt ultimateIndex = new(-1);
        public readonly NetBool isUltimateActive = new(false);
    }

    internal static ConditionalWeakTable<Farmer, Holder> Values = new();

    public static NetInt get_UltimateIndex(this Farmer farmer)
    {
        var holder = Values.GetOrCreateValue(farmer);
        return holder.ultimateIndex;
    }

    public static void set_UltimateIndex(this Farmer farmer, NetInt newVal)
    {
        // Net types should not have a setter as they are readonly
    }

    public static NetBool get_IsUltimateActive(this Farmer farmer)
    {
        var holder = Values.GetOrCreateValue(farmer);
        return holder.isUltimateActive;
    }

    public static void set_IsUltimateActive(this Farmer farmer, NetBool newVal)
    {
        // Net types should not have a setter as they are readonly
    }

    public static Ultimate? get_Ultimate(this Farmer farmer)
    {
        var holder = Values.GetOrCreateValue(farmer);
        return holder.ultimate;
    }

    public static void set_Ultimate(this Farmer farmer, Ultimate? newVal)
    {
        var holder = Values.GetOrCreateValue(farmer);
        holder.ultimate = newVal;

        var newIndex = newVal?.Index ?? UltimateIndex.None;
        holder.ultimateIndex.Value = (int)newIndex;
        ModDataIO.Write(farmer, "UltimateIndex", newIndex.ToString());
    }
}