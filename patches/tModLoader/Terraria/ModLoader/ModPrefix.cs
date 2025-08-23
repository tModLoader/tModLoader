using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

public enum PrefixCategory
{
	/// <summary>
	/// Can modify the size of the weapon
	/// </summary>
	Melee,
	/// <summary>
	/// Can modify the shoot speed of the weapon
	/// </summary>
	Ranged,
	/// <summary>
	/// Can modify the mana usage of the weapon
	/// </summary>
	Magic,
	AnyWeapon,
	Accessory,
	/// <summary>
	/// Will not appear by default. Useful as prefixes for your own damage type.
	/// </summary>
	Custom
}

/// <summary>
/// Represents a modded prefix (or modifier). The <see href="https://terraria.wiki.gg/wiki/Modifiers">Modifiers page on the Terraria wiki</see> is a good resource for vanilla prefixes.
/// </summary>
public abstract class ModPrefix : ModType, ILocalizedModType
{
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "Prefixes";

	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// The category your prefix belongs to, PrefixCategory.Custom by default
	/// </summary>
	public virtual PrefixCategory Category => PrefixCategory.Custom;

	protected sealed override void Register()
	{
		ModTypeLookup<ModPrefix>.Register(this);
		Type = PrefixLoader.ReservePrefixID();
		PrefixLoader.RegisterPrefix(this);
	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
		PrefixID.Search.Add(FullName, Type);
	}

	/// <summary>
	/// The roll chance of your prefix relative to a vanilla prefix, 1f by default.
	/// </summary>
	public virtual float RollChance(Item item) => 1f;

	/// <summary>
	/// Returns if your ModPrefix can roll on the given item
	/// By default returns RollChance(item) > 0
	/// </summary>
	public virtual bool CanRoll(Item item) => RollChance(item) > 0;

	/// <summary>
	/// Sets the stat changes for this prefix. If data is not already pre-stored, it is best to store custom data changes to some static variables.
	/// <br/><br/> All parameters default to 1f, except <paramref name="critBonus"/> which defaults to 0.
	/// <br/><br/> It is important to remember that a prefix will only be applied to an item if every stat it affects would be changed after multiplication and rounding to the nearest integer. If a ModPrefix is not being applied to items, a multiplier might be too insignificant. The <see href="https://terraria.wiki.gg/wiki/Modifiers#Notes">Modifiers page on the Terraria wiki</see> has more details.
	/// <br/><br/> Use <see cref="AllStatChangesHaveEffectOn"/> to implement that same restriction for modded stats. 
	/// </summary>
	public virtual void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
		ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) { }

	/// <summary>
	/// Use this to check whether modifiers to custom stats would be too small to have an effect after rounding, and prevent the prefix from being applied to the given item if there would be no change.
	/// <para/>Vanilla stat changes (<seealso cref="SetStats"/>) are checked automatically, so there is no need to override this method to check them
	/// </summary>
	/// <returns>false to prevent the prefix from being applied</returns>
	public virtual bool AllStatChangesHaveEffectOn(Item item) => true;

	/// <summary>
	/// Applies the custom data stats set in SetStats to the given item.
	/// </summary>
	public virtual void Apply(Item item) { }

	/// <summary>
	/// Allows you to modify the sell price of the item based on the prefix or changes in custom data stats. This also influences the item's rarity.
	/// </summary>
	public virtual void ModifyValue(ref float valueMult) { }

	/// <summary>
	/// Use this to modify player stats (or any other applicable data) based on this ModPrefix.
	/// </summary>
	/// <param name="player"> The player gaining the benefits of this accessory. </param>
	public virtual void ApplyAccessoryEffects(Player player) { }

	/// <summary>
	/// Use this to add tooltips to any item with this prefix applied. Note that the stat bonuses applied via <see cref="SetStats"/> will automatically generate tooltips. (such as damage, use speed, crit chance, mana cost, scale, shoot speed, and knockback)<br/>
	/// </summary>
	public virtual IEnumerable<TooltipLine> GetTooltipLines(Item item)
	{
		return null;
	}
}
