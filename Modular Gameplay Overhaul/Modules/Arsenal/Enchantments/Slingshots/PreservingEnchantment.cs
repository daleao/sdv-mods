namespace DaLion.Overhaul.Modules.Arsenal.Enchantments;

#region using directives

using System.Xml.Serialization;

#endregion using directives

/// <summary>Chance to not consume ammo.</summary>
[XmlType("Mods_DaLion_PreservingEnchantment")]
public sealed class PreservingEnchantment : BaseSlingshotEnchantment
{
    /// <inheritdoc />
    public override string GetName()
    {
        return I18n.Get("enchantments.preserving");
    }
}
