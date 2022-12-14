namespace DaLion.Overhaul.Modules.Arsenal.Patchers.Slingshots;

#region using directives

using System.Linq;
using System.Reflection;
using DaLion.Overhaul.Modules.Arsenal.Enchantments;
using DaLion.Overhaul.Modules.Arsenal.Events.Slingshots;
using DaLion.Overhaul.Modules.Arsenal.Extensions;
using DaLion.Overhaul.Modules.Arsenal.Projectiles;
using DaLion.Overhaul.Modules.Arsenal.VirtualProperties;
using DaLion.Overhaul.Modules.Professions.Events.GameLoop;
using DaLion.Overhaul.Modules.Professions.Extensions;
using DaLion.Overhaul.Modules.Professions.Ultimates;
using DaLion.Overhaul.Modules.Professions.VirtualProperties;
using DaLion.Shared.Extensions.Stardew;
using DaLion.Shared.Extensions.Xna;
using DaLion.Shared.Harmony;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.Projectiles;
using StardewValley.Tools;

#endregion using directives

[UsedImplicitly]
internal sealed class SlingshotPerformFirePatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="SlingshotPerformFirePatcher"/> class.</summary>
    internal SlingshotPerformFirePatcher()
    {
        this.Target = this.RequireMethod<Slingshot>(nameof(Slingshot.PerformFire));
        this.Prefix!.priority = Priority.High;
        this.Prefix!.after = new[] { OverhaulModule.Professions.Namespace };
    }

    #region harmony patches

    /// <summary>Patch to add Rascal bonus range damage + perform Desperado perks and Ultimate.</summary>
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyAfter("Overhaul.Modules.Professions")]
    private static bool SlingshotPerformFirePrefix(
        Slingshot __instance,
        ref bool ___canPlaySound,
        GameLocation location,
        Farmer who)
    {
        if (__instance.Get_IsOnSpecial())
        {
            return false; // don't run original logic
        }

        EventManager.Disable<BullseyeRenderedEvent>();

        try
        {
            var canDoQuincy = __instance.hasEnchantmentOfType<QuincyEnchantment>() && location.HasMonsters();
            if (__instance.attachments[0] is null && !canDoQuincy && !who.IsSteppingOnSnow())
            {
                if (__instance.attachments.Count > 1 && __instance.attachments[1] is not null)
                {
                    __instance.attachments[0] = __instance.attachments[1];
                    __instance.attachments[1] = null;
                }
                else
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
                    ___canPlaySound = true;
                    return false; // don't run original logic
                }
            }

            var backArmDistance = __instance.GetBackArmDistance(who);
            if (backArmDistance <= 4 || ___canPlaySound)
            {
                return false; // don't run original logic
            }

            // calculate projectile velocity
            Reflector
                .GetUnboundMethodDelegate<Action<Slingshot>>(__instance, "updateAimPos")
                .Invoke(__instance);
            var mouseX = __instance.aimPos.X;
            var mouseY = __instance.aimPos.Y;
            var shootOrigin = __instance.GetShootOrigin(who);
            var (xVelocity, yVelocity) = Utility.getVelocityTowardPoint(
                shootOrigin,
                __instance.AdjustForHeight(new Vector2(mouseX, mouseY)),
                Game1.random.Next(4, 6) + 15);

            // get and spend ammo
            var ammo = __instance.attachments[0]?.getOne();
            var didPreserve = false;
            if (ammo is not null && (!__instance.hasEnchantmentOfType<PreservingEnchantment>() || Game1.random.NextDouble() > 0.5 + (who.DailyLuck / 2) + (who.LuckLevel * 0.01)))
            {
                if (--__instance.attachments[0].Stack <= 0)
                {
                    __instance.attachments[0] = null;
                }
            }
            else
            {
                didPreserve = true;
            }

            int damageBase;
            float knockback;
            switch (ammo?.ParentSheetIndex)
            {
                case SObject.wood or SObject.coal:
                    damageBase = 2;
                    knockback = 0.3f;
                    break;
                case SObject.stone:
                    damageBase = 5;
                    knockback = 0.5f;
                    break;
                case SObject.copper:
                    damageBase = 10;
                    knockback = 0.525f;
                    break;
                case SObject.iron:
                    damageBase = 20;
                    knockback = 0.55f;
                    break;
                case SObject.gold:
                    damageBase = 30;
                    knockback = 0.575f;
                    break;
                case SObject.iridium:
                    damageBase = 50;
                    knockback = 0.6f;
                    break;
                case Constants.RadioactiveOreIndex:
                    damageBase = 80;
                    knockback = 0.625f;
                    break;
                case Constants.ExplosiveAmmoIndex:
                    damageBase = 5;
                    knockback = 0.4f;
                    break;
                case Constants.SlimeIndex:
                    damageBase = who.professions.Contains(Farmer.acrobat) ? 10 : 5;
                    knockback = 0f;
                    break;
                case null: // quincy or snowball
                    damageBase = canDoQuincy ? 5 : 1;
                    knockback = canDoQuincy ? 0f : 0.4f;
                    break;
                default: // fish, fruit or vegetable
                    damageBase = 1;
                    knockback = 0f;
                    break;
            }

            // apply slingshot damage modifiers
            float damageMod;
            switch (__instance.InitialParentTileIndex)
            {
                case Constants.MasterSlingshotIndex:
                    damageMod = 1.5f;
                    knockback += 0.1f;
                    break;
                case Constants.GalaxySlingshotIndex:
                    damageMod = 2f;
                    knockback += 0.2f;
                    break;
                case Constants.InfinitySlingshotIndex:
                    damageMod = 2.5f;
                    knockback += 0.25f;
                    break;
                default:
                    damageMod = 1f;
                    break;
            }

            // calculate overcharge
            var overcharge = ProfessionsModule.IsEnabled && who.professions.Contains(Farmer.desperado)
                ? __instance.GetOvercharge()
                : 1f;

            // adjust velocity
            if (overcharge > 1f)
            {
                xVelocity *= overcharge;
                yVelocity *= overcharge;
                EventManager.Disable<DesperadoUpdateTickedEvent>();
            }

            if (Game1.options.useLegacySlingshotFiring)
            {
                xVelocity *= -1f;
                yVelocity *= -1f;
            }

            // add main projectile
            var damage = (damageBase + Game1.random.Next(-damageBase / 2, damageBase + 2)) * damageMod;
            var startingPosition = shootOrigin - new Vector2(32f, 32f);
            var rotationVelocity = (float)(Math.PI / (64f + Game1.random.Next(-63, 64)));
            var index = ammo?.ParentSheetIndex ?? (canDoQuincy
                ? QuincyProjectile.TileSheetIndex
                : Projectile.snowBall);
            if (ammo is not null && ammo.ParentSheetIndex is not (Constants.ExplosiveAmmoIndex or Constants.SlimeIndex
                    or Constants.RadioactiveOreIndex) && damageBase > 1)
            {
                index++;
            }

            BasicProjectile projectile = index switch
            {
                QuincyProjectile.TileSheetIndex => new QuincyProjectile(
                    __instance,
                    who,
                    damage,
                    overcharge,
                    startingPosition,
                    xVelocity,
                    yVelocity,
                    rotationVelocity),
                Projectile.snowBall => new SnowballProjectile(
                    who,
                    overcharge,
                    startingPosition,
                    xVelocity,
                    yVelocity,
                    rotationVelocity),
                _ => new ObjectProjectile(
                    ammo!,
                    index,
                    __instance,
                    who,
                    damage,
                    knockback,
                    overcharge,
                    startingPosition,
                    xVelocity,
                    yVelocity,
                    rotationVelocity,
                    !didPreserve),
            };

            if (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
            {
                projectile.IgnoreLocationCollision = true;
            }

            if (__instance.hasEnchantmentOfType<EngorgingEnchantment>())
            {
                projectile.startingScale.Value *= 2f;
            }

            location.projectiles.Add(projectile);
            ___canPlaySound = true;

            // do Death Blossom
            if (who.Get_Ultimate() is DeathBlossom { IsActive: true })
            {
                var velocity = new Vector2(xVelocity, yVelocity);
                for (var i = 0; i < 7; i++)
                {
                    velocity = velocity.Rotate(45);
                    if (i == 3)
                    {
                        continue;
                    }

                    damage = (damageBase + Game1.random.Next(-damageBase / 2, damageBase + 2)) * damageMod;
                    rotationVelocity = (float)(Math.PI / (64f + Game1.random.Next(-63, 64)));
                    BasicProjectile petal = index switch
                    {
                        QuincyProjectile.TileSheetIndex => new QuincyProjectile(
                            __instance,
                            who,
                            damage,
                            0f,
                            startingPosition,
                            velocity.X,
                            velocity.Y,
                            rotationVelocity),
                        Projectile.snowBall => new SnowballProjectile(
                            who,
                            0f,
                            startingPosition,
                            velocity.X,
                            velocity.Y,
                            rotationVelocity),
                        _ => new ObjectProjectile(
                            ammo!,
                            index,
                            __instance,
                            who,
                            damage,
                            knockback,
                            0f,
                            startingPosition,
                            velocity.X,
                            velocity.Y,
                            rotationVelocity,
                            false),
                    };

                    if (Game1.currentLocation.currentEvent != null || Game1.currentMinigame != null)
                    {
                        petal.IgnoreLocationCollision = true;
                    }

                    who.currentLocation.projectiles.Add(petal);
                }
            }

            foreach (var enchantment in __instance.enchantments.OfType<BaseSlingshotEnchantment>())
            {
                enchantment.OnFire(
                    __instance,
                    projectile,
                    damageBase,
                    damageMod,
                    knockback,
                    startingPosition,
                    xVelocity,
                    yVelocity,
                    location,
                    who);
            }

            return false; // don't run original logic
        }
        catch (Exception ex)
        {
            Log.E($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}");
            return true; // default to original logic
        }
    }

    #endregion harmony patches
}
