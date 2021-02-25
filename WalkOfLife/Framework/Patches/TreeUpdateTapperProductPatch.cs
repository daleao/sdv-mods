﻿using Harmony;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TheLion.Common.Harmony;

using static TheLion.AwesomeProfessions.Framework.Utils;

namespace TheLion.AwesomeProfessions.Framework.Patches
{
	internal class TreeUpdateTapperProductPatch : BasePatch
	{
		private static ILHelper _helper;

		/// <summary>Construct an instance.</summary>
		/// <param name="config">The mod settings.</param>
		/// <param name="monitor">Interface for writing to the SMAPI console.</param>
		internal TreeUpdateTapperProductPatch(ModConfig config, IMonitor monitor)
		: base(config, monitor)
		{
			_helper = new ILHelper(monitor);
		}

		/// <summary>Apply internally-defined Harmony patches.</summary>
		/// <param name="harmony">The Harmony instance for this mod.</param>
		protected internal override void Apply(HarmonyInstance harmony)
		{
			harmony.Patch(
				AccessTools.Method(typeof(Tree), nameof(Tree.UpdateTapperProduct)),
				transpiler: new HarmonyMethod(GetType(), nameof(TreeUpdateTapperProductTranspiler))
			);
		}

		/// <summary>Patch for Tapper syrup production time.</summary>
		protected static IEnumerable<CodeInstruction> TreeUpdateTapperProductTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
		{
			_helper.Attach(instructions).Log($"Patching method {typeof(Tree)}::{nameof(Tree.UpdateTapperProduct)}.");

			/// Injected: if (Game1.player.professions.Contains(<tapper_id>) time_multiplier *= 0.75
			
			Label tapperCheck = iLGenerator.DefineLabel();
			Label isNotTapper = iLGenerator.DefineLabel();
			try
			{
				_helper
					.Find(
						new CodeInstruction(OpCodes.Brfalse)
					)
					.SetOperand(tapperCheck)						// set branch destination to tapper check
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Bne_Un)
					)
					.SetOperand(tapperCheck)						// set branch destination to tapper check
					.AdvanceUntil(
						new CodeInstruction(OpCodes.Ldarg_0)
					)
					.AddLabel(isNotTapper)							// branch here if player is not tapper
					.InsertProfessionCheck(ProfessionsMap.Forward["tapper"], branchDestination: isNotTapper, label: tapperCheck)
					.Insert(
						// multiply local 0 by 0.75
						new CodeInstruction(OpCodes.Ldc_R4, operand: 0.75f),
						new CodeInstruction(OpCodes.Ldloc_0),		// local 0 = time_multiplier
						new CodeInstruction(OpCodes.Mul),
						new CodeInstruction(OpCodes.Stloc_0)
					);
			}
			catch (Exception ex)
			{
				_helper.Restore().Error($"Failed while patching Tapper syrup production.\nHelper returned {ex}");
			}

			return _helper.Flush();
		}
	}
}
