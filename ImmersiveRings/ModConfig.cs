﻿namespace DaLion.Stardew.Rings;

/// <summary>The mod user-defined settings.</summary>
public class ModConfig
{
    /// <summary>Improves certain underwhelming rings.</summary>
    public bool RebalancedRings { get; set; } = true;

    /// <summary>Adds new combat recipes for crafting gemstone rings.</summary>
    public bool CraftableGemRings { get; set; } = true;

    /// <summary>Adds new mining recipes for crafting glow and magnet rings.</summary>
    public bool CraftableGlowAndMagnetRings { get; set; } = true;

    /// <summary>Replaces the glowstone ring recipe.</summary>
    public bool ImmersiveGlowstoneRecipe { get; set; } = true;

    /// <summary>Replaces the iridium band recipe and effect. Adds new forge mechanics.</summary>
    public bool ForgeableIridiumBand { get; set; } = true;
}