namespace DaLion.Overhaul.Modules.Arsenal.Enchantments;

#region using directives

using System.Xml.Serialization;
using DaLion.Overhaul.Modules.Arsenal.Projectiles;
using DaLion.Shared.Enums;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using StardewValley.Tools;

#endregion using directives

/// <summary>The secondary <see cref="BaseWeaponEnchantment"/> which characterizes Infinity weapons.</summary>
[XmlType("Mods_DaLion_InfinityEnchantment")]
public class InfinityEnchantment : BaseWeaponEnchantment
{
    /// <inheritdoc />
    public override bool IsSecondaryEnchantment()
    {
        return true;
    }

    /// <inheritdoc />
    public override bool IsForge()
    {
        return false;
    }

    /// <inheritdoc />
    public override int GetMaximumLevel()
    {
        return 1;
    }

    /// <inheritdoc />
    public override bool ShouldBeDisplayed()
    {
        return false;
    }

    /// <inheritdoc />
    public override bool CanApplyTo(Item item)
    {
        return item is Tool tool && tool.GetEnchantmentLevel<GalaxySoulEnchantment>() >= 3;
    }

    /// <inheritdoc />
    protected override void _OnSwing(MeleeWeapon weapon, Farmer farmer)
    {
        base._OnSwing(weapon, farmer);
        if (farmer.health < farmer.maxHealth)
        {
            return;
        }

        var facingDirection = (FacingDirection)farmer.FacingDirection;
        var facingVector = facingDirection.ToVector();
        var startingPosition = farmer.getStandingPosition() + (facingVector * 64f) - new Vector2(32f, 32f);
        var velocity = facingVector * 10f;
        var rotation = (float)Math.PI / 180f * 32f;
        farmer.currentLocation.projectiles.Add(new InfinityProjectile(
            weapon,
            farmer,
            startingPosition,
            velocity.X,
            velocity.Y,
            rotation));
    }

    /// <inheritdoc />
    protected override void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
    {
        var monsterBox = monster.GetBoundingBox();
        var tempSprite = new TemporaryAnimatedSprite(
            360,
            Game1.random.Next(50, 120),
            2,
            2,
            new Vector2(monsterBox.Center.X - 32, monsterBox.Center.Y - 32),
            flicker: false,
            flipped: false);
        tempSprite.color = Color.HotPink;

        Reflector
            .GetStaticFieldGetter<Multiplayer>(typeof(Game1), "multiplayer")
            .Invoke()
            .broadcastSprites(location, tempSprite);
    }
}
