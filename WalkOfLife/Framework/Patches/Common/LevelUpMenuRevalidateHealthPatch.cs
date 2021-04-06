﻿using Harmony;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TheLion.AwesomeProfessions
{
	internal class LevelUpMenuRevalidateHealthPatch : BasePatch
	{
		/// <inheritdoc/>
		public override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.RevalidateHealth)),
				transpiler: new HarmonyMethod(GetType(), nameof(LevelUpMenuRevalidateHealthTranspiler)),
				postfix: new HarmonyMethod(GetType(), nameof(LevelUpMenuRevalidateHealthPostfix))
			);
		}

		#region harmony patches

		/// <summary>Patch to move bonus health from Defender to Brute.</summary>
		private static IEnumerable<CodeInstruction> LevelUpMenuRevalidateHealthTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			Helper.Attach(instructions).Log($"Patching method {typeof(LevelUpMenu)}::{nameof(LevelUpMenu.RevalidateHealth)}.");

			/// From: if (farmer.professions.Contains(<defender_id>))
			/// To: if (farmer.professions.Contains(<brute_id>))

			try
			{
				Helper
					.FindProfessionCheck(Farmer.defender)
					.Advance()
					.SetOperand(Utility.ProfessionMap.Forward["Brute"]);
			}
			catch (Exception ex)
			{
				Helper.Error($"Failed while moving vanilla Defender health bonus to Brute.\nHelper returned {ex}").Restore();
			}

			return Helper.Flush();
		}

		/// <summary>Patch revalidate modded immediate profession perks.</summary>
		private static void LevelUpMenuRevalidateHealthPostfix(Farmer farmer)
		{
			// revalidate tackle health
			int expectedMaxTackleUses = 20;
			if (Utility.SpecificPlayerHasProfession("Angler", farmer)) expectedMaxTackleUses *= 2;

			FishingRod.maxTackleUses = expectedMaxTackleUses;

			// revalidate fish pond capacity
			foreach (FishPond pond in Game1.getFarm().buildings.Where(b => (b.owner.Value.Equals(farmer.UniqueMultiplayerID) || !Game1.IsMultiplayer) && b is FishPond))
			{
				pond.UpdateMaximumOccupancy();
				pond.currentOccupants.Value = Math.Min(pond.currentOccupants.Value, pond.maxOccupants.Value);
			}
		}

		#endregion harmony patches
	}
}