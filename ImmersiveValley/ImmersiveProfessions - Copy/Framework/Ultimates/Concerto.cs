﻿namespace DaLion.Stardew.Professions.Framework.Ultimates;

#region using directives

using Common.ModData;
using Events.GameLoop;
using Extensions;
using Microsoft.Xna.Framework;
using Sounds;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using System.Linq;

#endregion using directives

/// <summary>Handles Piper ultimate activation.</summary>
public sealed class Concerto : Ultimate
{
    /// <summary>Construct an instance.</summary>
    internal Concerto()
    : base(UltimateIndex.PiperConcerto, Color.LimeGreen, Color.DarkGreen) { }

    #region public properties

    /// <inheritdoc />
    public override bool CanActivate => base.CanActivate && Game1.player.currentLocation.characters.OfType<Monster>()
        .Any(m => m.IsSlime() && m.IsWithinPlayerThreshold());

    #endregion public properties

    #region internal properties

    /// <inheritdoc />
    internal override SFX ActivationSfx => SFX.PiperConcerto;

    /// <inheritdoc />
    internal override Color GlowColor => Color.LimeGreen;

    #endregion internal properties

    #region internal methods

    /// <inheritdoc />
    internal override void Activate()
    {
        base.Activate();

        foreach (var slime in Game1.player.currentLocation.characters.OfType<GreenSlime>().Where(c =>
                     c.IsWithinPlayerThreshold() && c.Scale < 2f && !ModDataIO.Read<bool>(c, "Piped")))
        {
            if (Game1.random.NextDouble() <= 0.012 + Game1.player.team.AverageDailyLuck() / 10.0)
            {
                if (Game1.currentLocation is MineShaft && Game1.player.team.SpecialOrderActive("Wizard2"))
                    slime.makePrismatic();
                else slime.hasSpecialItem.Value = true;
            }

            slime.MakePipedSlime(Game1.player);
            ModEntry.Player.PipedSlimes.Add(slime);
        }

        var bigSlimes = Game1.currentLocation.characters.OfType<BigSlime>().ToList();
        for (var i = bigSlimes.Count - 1; i >= 0; --i)
        {
            bigSlimes[i].Health = 0;
            bigSlimes[i].deathAnimation();
            var toCreate = Game1.random.Next(2, 5);
            while (toCreate-- > 0)
            {
                Game1.currentLocation.characters.Add(new GreenSlime(bigSlimes[i].Position, Game1.CurrentMineLevel));
                var justCreated = Game1.currentLocation.characters[^1];
                justCreated.setTrajectory((int)(bigSlimes[i].xVelocity / 8 + Game1.random.Next(-2, 3)),
                    (int)(bigSlimes[i].yVelocity / 8 + Game1.random.Next(-2, 3)));
                justCreated.willDestroyObjectsUnderfoot = false;
                justCreated.moveTowardPlayer(4);
                justCreated.Scale = 0.75f + Game1.random.Next(-5, 10) / 100f;
                justCreated.currentLocation = Game1.currentLocation;
            }
        }

        ModEntry.EventManager.Hook<SlimeInflationUpdateTickedEvent>();
        ActivationSfx.PlayAfterDelay(333);
    }

    ///// <inheritdoc />
    //internal override void Activate()
    //{
    //    if (ChargeValue < _InflationCost)
    //    {
    //        Game1.playSound("cancel");
    //        return;
    //    }

    //    ModEntry.EventManager.Unhook<UltimateGaugeShakeUpdateTickedEvent>();
    //    ActivationSfx.Play();
    //    foreach (var slime in Game1.player.currentLocation.characters.OfType<GreenSlime>().Where(c =>
    //                 c.IsWithinPlayerThreshold() && c.Scale < 2f && !ModDataIO.Read<bool>(c, "Piped")))
    //    {
    //        if (ChargeValue < _InflationCost) break;
    //        ChargeValue -= _InflationCost;

    //        if (Game1.random.NextDouble() <= 0.012 + Game1.player.team.AverageDailyLuck() / 10.0)
    //        {
    //            if (Game1.currentLocation is MineShaft && Game1.player.team.SpecialOrderActive("Wizard2"))
    //                slime.makePrismatic();
    //            else slime.hasSpecialItem.Value = true;
    //        }

    //        slime.MakePipedSlime(Game1.player);
    //        ModEntry.PlayerState.PipedSlimes.Add(slime);
    //    }

    //    var bigSlimes = Game1.currentLocation.characters.OfType<BigSlime>().ToList();
    //    for (var i = bigSlimes.Count - 1; i >= 0; --i)
    //    {
    //        bigSlimes[i].Health = 0;
    //        bigSlimes[i].deathAnimation();
    //        var toCreate = Game1.random.Next(2, 5);
    //        while (toCreate-- > 0)
    //        {
    //            Game1.currentLocation.characters.Add(new GreenSlime(bigSlimes[i].Position, Game1.CurrentMineLevel));
    //            var justCreated = Game1.currentLocation.characters[^1];
    //            justCreated.setTrajectory((int)(bigSlimes[i].xVelocity / 8 + Game1.random.Next(-2, 3)),
    //                (int)(bigSlimes[i].yVelocity / 8 + Game1.random.Next(-2, 3)));
    //            justCreated.willDestroyObjectsUnderfoot = false;
    //            justCreated.moveTowardPlayer(4);
    //            justCreated.Scale = 0.75f + Game1.random.Next(-5, 10) / 100f;
    //            justCreated.currentLocation = Game1.currentLocation;
    //        }
    //    }

    //    ModEntry.EventManager.Hook<SlimeInflationUpdateTickedEvent>();
    //    ModEntry.EventManager.Hook<SlimeDeflationUpdateTickedEvent>();
    //    ModEntry.EventManager.Hook<UltimateActiveUpdateTickedEvent>();
    //}

    /// <inheritdoc />
    internal override void Deactivate()
    {
        base.Deactivate();
        ModEntry.EventManager.Hook<SlimeDeflationUpdateTickedEvent>();
    }

    /// <inheritdoc />
    internal override void Countdown(double elapsed)
    {
        ChargeValue -= elapsed * 0.01 / 3.0; // lasts 30s

        var piped = ModEntry.Player.PipedSlimes;
        if (piped.Count <= 0)
        {
            ModEntry.EventManager.Unhook<UltimateActiveUpdateTickedEvent>();
            return;
        }

        foreach (var slime in piped) slime.Countdown(elapsed);
    }

    #endregion internal methods
}