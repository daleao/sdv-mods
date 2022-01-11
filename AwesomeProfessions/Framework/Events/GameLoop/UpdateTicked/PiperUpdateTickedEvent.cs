﻿using StardewModdingAPI.Events;
using StardewValley;
using TheLion.Stardew.Professions.Framework.Patches.Foraging;

namespace TheLion.Stardew.Professions.Framework.Events.GameLoop.UpdateTicked;

internal class PiperUpdateTickedEvent : UpdateTickedEvent
{
    /// <inheritdoc />
    protected override void OnUpdateTickedImpl(object sender, UpdateTickedEventArgs e)
    {
        if (ModEntry.State.Value.SlimeContactTimer > 0 &&
            Game1ShouldTimePassPatch.Game1ShouldTimePassOriginal(Game1.game1, true))
            --ModEntry.State.Value.SlimeContactTimer;
    }
}