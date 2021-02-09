﻿using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace TheLion.AwesomeTools.Framework
{
	internal static class HarmonyPatcher
	{
		private static readonly int[] _axeAffectedTilesRadii = ModEntry.Config.AxeConfig.RadiusAtEachLevel;
		private static readonly int[] _pickaxeAffectedTilesRadii = ModEntry.Config.PickaxeConfig.RadiusAtEachLevel;

		// Enable Axe power level increase
		[HarmonyPatch(typeof(Axe), "beginUsing")]
		internal class Before_Axe_BeginUsing
		{
			protected static bool Prefix(ref Tool __instance, Farmer who)
			{
				if ((ModEntry.Config.RequireHotkey && !ModEntry.Config.Hotkey.IsDown()))
					return true; // run original logic

				who.Halt();
				__instance.Update(who.FacingDirection, 0, who);
				switch (who.FacingDirection)
				{
					case 0:
						who.FarmerSprite.setCurrentFrame(176);
						__instance.Update(0, 0, who);
						break;
					case 1:
						who.FarmerSprite.setCurrentFrame(168);
						__instance.Update(1, 0, who);
						break;
					case 2:
						who.FarmerSprite.setCurrentFrame(160);
						__instance.Update(2, 0, who);
						break;
					case 3:
						who.FarmerSprite.setCurrentFrame(184);
						__instance.Update(3, 0, who);
						break;
				}
				return false; // don't run original logic
			}
		}

		// Enable Pickaxe power level increase
		[HarmonyPatch(typeof(Pickaxe), "beginUsing")]
		internal class Before_Pickaxe_BeginUsing
		{
			protected static bool Prefix(ref Tool __instance, Farmer who)
			{
				if (ModEntry.Config.RequireHotkey && !ModEntry.Config.Hotkey.IsDown())
					return true; // run original logic

				who.Halt();
				__instance.Update(who.FacingDirection, 0, who);
				switch (who.FacingDirection)
				{
					case 0:
						who.FarmerSprite.setCurrentFrame(176);
						__instance.Update(0, 0, who);
						break;
					case 1:
						who.FarmerSprite.setCurrentFrame(168);
						__instance.Update(1, 0, who);
						break;
					case 2:
						who.FarmerSprite.setCurrentFrame(160);
						__instance.Update(2, 0, who);
						break;
					case 3:
						who.FarmerSprite.setCurrentFrame(184);
						__instance.Update(3, 0, who);
						break;
				}
				return false; // don't run original logic
			}
		}

		// Allow first two power levels on Pickaxe
		[HarmonyPatch(typeof(Farmer), "toolPowerIncrease")]
		internal class During_Farmer_ToolPowerIncrease
		{
			protected static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var l = instructions.ToList();
				for (int i = 0; i < instructions.Count(); ++i)
				{
					if (l[i].opcode == OpCodes.Isinst && l[i].operand.ToString().Equals("StardewValley.Tools.Pickaxe"))
					{
						// inject logic: if (!this.CurrentTool.UpgradeLevel < 5) then don't run toolPower += 2
						//l.InsertRange(i + 2, l.GetRange(i - 2, 2)										// copy push this.CurrentTool to the stack
						//	.Concat(new[] { new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Tool), nameof(Tool.UpgradeLevel)).GetGetMethod()),	// call CurrentTool.UpgradeLevel_get
						//					new CodeInstruction(OpCodes.Ldc_I4_5),						// push the integer 5 onto the stack
						//					new CodeInstruction(OpCodes.Bge_Un_S, l[i + 1].operand) }	// if greater than or equal to, branch to next segment (skip toolPower += 2)
						//	)
						//);

						// inject logic: branch over toolPower += 2
						l.InsertRange(i - 2, new List<CodeInstruction> { new CodeInstruction(OpCodes.Br_S, l[i + 1].operand) });
						break;
					}
				}
				return l.AsEnumerable();
			}
		}

		// Set affected tiles for Axe and Pickaxe power levels
		[HarmonyPatch(typeof(Tool), "tilesAffected")]
		internal class After_Tool_TileseAffected
		{
			protected static void Postfix(ref Tool __instance, ref List<Vector2> __result, Vector2 tileLocation, int power, Farmer who)
			{
				if (__instance.UpgradeLevel < 1)
					return;

				if (__instance is Axe || __instance is Pickaxe)
				{
					__result.Clear();

					int radius = __instance is Axe ? _axeAffectedTilesRadii[Math.Min(power - 2, 4)] : _pickaxeAffectedTilesRadii[Math.Min(power - 2, 4)];
					if (radius == 0)
						return;

					foreach (Vector2 tile in Utils.GetTilesAround(tileLocation, radius))
					{
						__result.Add(tile);
					}
				}
			}
		}

		//// Hide affected tiles overlay of Axe and Pickaxe
		//[HarmonyPatch(typeof(Tool), "draw")]
		//internal class Before_Tool_Draw
		//{
		//	protected static bool Prefix(ref Tool __instance)
		//	{
		//		if (__instance is Axe || __instance is Pickaxe)
		//		{
		//			return false;
		//		}
		//		return true;
		//	}
		//}

		// Prevent shockwave from triggering the "tool isn't strong enough"
		[HarmonyPatch(typeof(ResourceClump), "performToolAction")]
		internal class Before_ResourceClump_PerformToolAction
		{
			protected static bool Prefix(ref ResourceClump __instance, ref bool __result, Tool t, int damage, Vector2 tileLocation, GameLocation location)
			{
				if (!ModEntry.IsDoingShockwave)
					return true; // run original logic

				switch (__instance.parentSheetIndex.Value)
				{
					case ResourceClump.hollowLogIndex:
						if (t is Axe && t.UpgradeLevel < 2)
						{
							__result = false;
							return false; // don't run original logic
						}
						break;
					case ResourceClump.meteoriteIndex:
						if (t is Pickaxe && t.UpgradeLevel < 3)
						{
							__result = false;
							return false; // don't run original logic
						}
						break;
					case ResourceClump.boulderIndex:
						if (t is Pickaxe && t.UpgradeLevel < 2)
						{
							__result = false;
							return false; // don't run original logic
						}
						break;
					default:
						return true; // run original logic
				}
				return true; // run original logic
			}
		}
	}
}