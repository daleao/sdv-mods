﻿namespace DaLion.Stardew.FishPonds.Framework.Extensions;

#region using directives

using StardewModdingAPI;
using StardewValley;
using Common.Extensions;

#endregion using directives

/// <summary>Extensions for the <see cref="Farmer"/> class.</summary>
internal static class FarmerExtensions
{
    /// <summary>Read a string from the <see cref="ModDataDictionary" />.</summary>
    /// <param name="field">The field to read from.</param>
    /// <param name="defaultValue">The default value to return if the field does not exist.</param>
    internal static string ReadData(this Farmer farmer, DataField field, string defaultValue = "")
    {
        return Game1.MasterPlayer.modData.Read($"{ModEntry.Manifest.UniqueID}/{farmer.UniqueMultiplayerID}/{field}",
            defaultValue);
    }

    /// <summary>Read a field from the <see cref="ModDataDictionary" /> as <typeparamref name="T" />.</summary>
    /// <param name="field">The field to read from.</param>
    /// <param name="defaultValue"> The default value to return if the field does not exist.</param>
    internal static T ReadDataAs<T>(this Farmer farmer, DataField field, T defaultValue = default)
    {
        return Game1.MasterPlayer.modData.ReadAs($"{ModEntry.Manifest.UniqueID}/{farmer.UniqueMultiplayerID}/{field}",
            defaultValue);
    }

    /// <summary>Write to a field in the <see cref="ModDataDictionary" />, or remove the field if supplied with a null or empty value.</summary>
    /// <param name="field">The field to write to.</param>
    /// <param name="value">The value to write, or <c>null</c> to remove the field.</param>
    internal static void WriteData(this Farmer farmer, DataField field, string value)
    {
        if (Context.IsMultiplayer && !Context.IsMainPlayer)
        {
            // request the main player
            ModEntry.ModHelper.Multiplayer.SendMessage(value, $"RequestUpdateData/Write/{field}",
                new[] { ModEntry.Manifest.UniqueID }, new[] { Game1.MasterPlayer.UniqueMultiplayerID });
            return;
        }

        Game1.player.modData.Write($"{ModEntry.Manifest.UniqueID}/{farmer.UniqueMultiplayerID}/{field}", value);
        Log.D($"[ModData]: Wrote {value} to {farmer.Name}'s {field}.");
    }

    /// <summary>Write to a field in the <see cref="ModDataDictionary" />, only if it doesn't yet have a value.</summary>
    /// <param name="field">The field to write to.</param>
    /// <param name="value">The value to write, or <c>null</c> to remove the field.</param>
    internal static bool WriteDataIfNotExists(this Farmer farmer, DataField field, string value)
    {
        if (Game1.MasterPlayer.modData.ContainsKey($"{ModEntry.Manifest.UniqueID}/{farmer.UniqueMultiplayerID}/{field}"))
        {
            Log.D($"[ModData]: The data field {field} already existed.");
            return true;
        }

        if (Context.IsMultiplayer && !Context.IsMainPlayer)
            ModEntry.ModHelper.Multiplayer.SendMessage(value, $"RequestUpdateData/Write/{field}",
                new[] { ModEntry.Manifest.UniqueID },
                new[] { Game1.MasterPlayer.UniqueMultiplayerID }); // request the main player
        else Game1.player.WriteData(field, value);

        return false;
    }

    /// <summary>Increment the value of a numeric field in the <see cref="ModDataDictionary" /> by an arbitrary amount.</summary>
    /// <param name="field">The field to update.</param>
    /// <param name="amount">Amount to increment by.</param>
    internal static void IncrementData<T>(this Farmer farmer, DataField field, T amount)
    {
        if (Context.IsMultiplayer && !Context.IsMainPlayer)
        {
            // request the main player
            ModEntry.ModHelper.Multiplayer.SendMessage(amount, $"RequestUpdateData/Increment/{field}",
                new[] { ModEntry.Manifest.UniqueID }, new[] { Game1.MasterPlayer.UniqueMultiplayerID });
            return;
        }

        Game1.player.modData.Increment($"{ModEntry.Manifest.UniqueID}/{farmer.UniqueMultiplayerID}/{field}", amount);
        Log.D($"[ModData]: Incremented {farmer.Name}'s {field} by {amount}.");
    }

    /// <summary>Increment the value of a numeric field in the <see cref="ModDataDictionary" /> by 1.</summary>
    /// <param name="field">The field to update.</param>
    internal static void IncrementData<T>(this Farmer farmer, DataField field)
    {
        if (Context.IsMultiplayer && !Context.IsMainPlayer)
        {
            // request the main player
            ModEntry.ModHelper.Multiplayer.SendMessage(1, $"RequestUpdateData/Increment/{field}",
                new[] { ModEntry.Manifest.UniqueID }, new[] { Game1.MasterPlayer.UniqueMultiplayerID });
            return;
        }

        Game1.player.modData.Increment($"{ModEntry.Manifest.UniqueID}/{farmer.UniqueMultiplayerID}/{field}",
            "1".Parse<T>());
        Log.D($"[ModData]: Incremented {farmer.Name}'s {field} by 1.");
    }
}