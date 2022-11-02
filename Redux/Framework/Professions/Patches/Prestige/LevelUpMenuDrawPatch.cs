﻿namespace DaLion.Redux.Framework.Professions.Patches.Prestige;

#region using directives

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using DaLion.Redux.Framework.Professions.Extensions;
using DaLion.Shared.Extensions.Reflection;
using DaLion.Shared.Harmony;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using HarmonyPatch = DaLion.Shared.Harmony.HarmonyPatch;

#endregion using directives

[UsedImplicitly]
internal sealed class LevelUpMenuDrawPatch : HarmonyPatch
{
    /// <summary>Initializes a new instance of the <see cref="LevelUpMenuDrawPatch"/> class.</summary>
    internal LevelUpMenuDrawPatch()
    {
        this.Target = this.RequireMethod<LevelUpMenu>(nameof(LevelUpMenu.draw), new[] { typeof(SpriteBatch) });
    }

    #region harmony patches

    /// <summary>Patch to increase the height of Level Up Menu to fit longer profession descriptions.</summary>
    [HarmonyPrefix]
    private static void LevelUpMenuDrawPrefix(LevelUpMenu __instance, int ___currentLevel)
    {
        if (__instance.isProfessionChooser && ___currentLevel == 10)
        {
            __instance.height += 16;
        }
    }

    /// <summary>Patch to draw Prestige tooltip during profession selection.</summary>
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? LevelUpMenuDrawTranspiler(
        IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var helper = new IlHelper(original, instructions);

        // From: string chooseProfession = Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
        // To: string chooseProfession = GetChooseProfessionText(this);
        try
        {
            helper
                .FindFirst(new CodeInstruction(OpCodes.Stloc_1))
                .RetreatUntil(new CodeInstruction(OpCodes.Ldsfld))
                .RemoveInstructionsUntil(new CodeInstruction(OpCodes.Callvirt))
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(LevelUpMenu).RequireField("currentLevel")),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(LevelUpMenuDrawPatch).RequireMethod(nameof(GetChooseProfessionText))));
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching level up menu choose profession text.\nHelper returned {ex}");
            return null;
        }

        // Injected: DrawSubroutine(this, b);
        // Before: else if (!isProfessionChooser)
        try
        {
            helper
                .FindFirst(
                    new CodeInstruction(
                        OpCodes.Ldfld,
                        typeof(LevelUpMenu).RequireField(nameof(LevelUpMenu.isProfessionChooser))))
                .Advance()
                .GetOperand(out var isNotProfessionChooser)
                .FindLabel((Label)isNotProfessionChooser)
                .Retreat()
                .InsertInstructions(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, typeof(LevelUpMenu).RequireField("currentLevel")),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(
                        OpCodes.Call,
                        typeof(LevelUpMenuDrawPatch).RequireMethod(nameof(DrawSubroutine))));
        }
        catch (Exception ex)
        {
            Log.E($"Failed while patching level up menu prestige tooltip draw.\nHelper returned {ex}");
            return null;
        }

        return helper.Flush();
    }

    #endregion harmony patches

    #region injected subroutines

    private static string GetChooseProfessionText(int currentLevel)
    {
        return currentLevel > 10
            ? ModEntry.i18n.Get("prestige.levelup.prestige")
            : Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
    }

    private static void DrawSubroutine(LevelUpMenu menu, int currentLevel, SpriteBatch b)
    {
        if (!ModEntry.Config.Professions.EnablePrestige || !menu.isProfessionChooser || currentLevel > 10)
        {
            return;
        }

        var professionsToChoose = ModEntry.Reflector
            .GetUnboundFieldGetter<LevelUpMenu, List<int>>(menu, "professionsToChoose")
            .Invoke(menu);
        if (!Profession.TryFromValue(professionsToChoose[0], out var leftProfession) ||
            !Profession.TryFromValue(professionsToChoose[1], out var rightProfession))
        {
            return;
        }

        Rectangle selectionArea;
        if (Game1.player.HasProfession(leftProfession) &&
            Game1.player.HasAllProfessionsBranchingFrom(leftProfession))
        {
            selectionArea = new Rectangle(
                menu.xPositionOnScreen + 32,
                menu.yPositionOnScreen + 232,
                (menu.width / 2) - 40,
                menu.height - 264);
            b.Draw(Game1.staminaRect, selectionArea, new Color(Color.Black, 0.3f));

            if (selectionArea.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                var hoverText = ModEntry.i18n.Get(leftProfession % 6 <= 1
                    ? "prestige.levelup.tooltip:5"
                    : "prestige.levelup.tooltip:10");
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }
        }

        if (Game1.player.HasProfession(rightProfession) &&
            Game1.player.HasAllProfessionsBranchingFrom(rightProfession))
        {
            selectionArea = new Rectangle(
                menu.xPositionOnScreen + (menu.width / 2) + 8,
                menu.yPositionOnScreen + 232,
                (menu.width / 2) - 40,
                menu.height - 264);
            b.Draw(Game1.staminaRect, selectionArea, new Color(Color.Black, 0.3f));

            if (selectionArea.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                var hoverText = ModEntry.i18n.Get(leftProfession % 6 <= 1
                    ? "prestige.levelup.tooltip:5"
                    : "prestige.levelup.tooltip:10");
                IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
            }
        }
    }

    #endregion injected subroutines
}