namespace DaLion.Overhaul.Modules.Arsenal.Patchers.Infinity;

#region using directives

using DaLion.Overhaul.Modules.Arsenal.Extensions;
using DaLion.Shared.Extensions.Stardew;
using DaLion.Shared.Harmony;
using HarmonyLib;
using StardewValley.Locations;

#endregion using directives

[UsedImplicitly]
internal sealed class AdventurerGuildGilPatcher : HarmonyPatcher
{
    /// <summary>Initializes a new instance of the <see cref="AdventurerGuildGilPatcher"/> class.</summary>
    internal AdventurerGuildGilPatcher()
    {
        this.Target = this.RequireMethod<AdventureGuild>("gil");
    }

    #region harmony patches

    /// <summary>Record Gil flag.</summary>
    [HarmonyPostfix]
    private static void AdventurerGuildGilPostfix()
    {
        var player = Game1.player;
        if (player.NumMonsterSlayerQuestsCompleted() < 5)
        {
            return;
        }

        player.WriteIfNotExists(DataFields.ProvenValor, true.ToString());
        Virtue.Valor.CheckForCompletion(player);
    }

    #endregion harmony patches
}
