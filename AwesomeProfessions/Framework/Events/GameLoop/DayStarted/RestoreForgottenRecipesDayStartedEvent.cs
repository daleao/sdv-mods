﻿using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;
using TheLion.Stardew.Common.Extensions;

namespace TheLion.Stardew.Professions.Framework.Events.GameLoop.DayStarted;

internal class RestoreForgottenRecipesDayStartedEvent : DayStartedEvent
{
    /// <inheritdoc />
    public override void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        var forgottenRecipes = ModData.Read("ForgottenRecipes").ToDictionary<string, int>(",", ";");
        if (!forgottenRecipes.Any())
        {
            ModEntry.Subscriber.Unsubscribe(GetType());
            return;
        }

        for (var i = forgottenRecipes.Count - 1; i >= 0; --i)
        {
            var key = forgottenRecipes.ElementAt(i).Key;
            if (Game1.player.craftingRecipes.ContainsKey(key))
            {
                Game1.player.craftingRecipes[key] += forgottenRecipes[key];
                forgottenRecipes.Remove(key);
            }
            else if (Game1.player.cookingRecipes.ContainsKey(key))
            {
                Game1.player.cookingRecipes[key] += forgottenRecipes[key];
                forgottenRecipes.Remove(key);
            }
        }

        ModData.Write("ForgottenRecipes", forgottenRecipes.Any()
            ? forgottenRecipes.ToString(",", ";")
            : null);
        ModEntry.Subscriber.Unsubscribe(GetType());
    }
}