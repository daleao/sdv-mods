﻿using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace TheLion.AwesomeProfessions
{
	internal class ProspectorHuntRenderedHudEvent : RenderedHudEvent
	{
		/// <inheritdoc/>
		public override void OnRenderedHud(object sender, RenderedHudEventArgs e)
		{
			// reveal treasure hunt target
			if (AwesomeProfessions.ProspectorHunt.TreasureTile != null)
			{
				float distanceSquared = (Game1.player.getTileLocation() - AwesomeProfessions.ProspectorHunt.TreasureTile.Value).LengthSquared();
				if (distanceSquared <= Math.Pow(AwesomeProfessions.Config.TreasureTileDetectionDistance, 2))
					Utility.DrawArrowPointerOverTarget(AwesomeProfessions.ProspectorHunt.TreasureTile.Value, Color.Violet);
			}
		}
	}
}