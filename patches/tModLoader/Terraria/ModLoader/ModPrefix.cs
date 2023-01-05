using System.Text.RegularExpressions;
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

public abstract class ModPrefix : ModType, ILocalizedModType
{
	public int Type { get; internal set; }

	public string LocalizationCategory => "Prefix";

	public virtual LocalizedText DisplayName => this.GetOrRegisterLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// The category your prefix belongs to, PrefixCategory.Custom by default
	/// </summary>
	public virtual PrefixCategory Category => PrefixCategory.Custom;

	protected sealed override void Register()
	{
		ModTypeLookup<ModPrefix>.Register(this);

		Type = PrefixLoader.ReservePrefixID();
		// DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"Prefix.{Name}");

		PrefixLoader.RegisterPrefix(this);
	}

	public sealed override void SetupContent()
	{
		AutoStaticDefaults();
		SetStaticDefaults();
	}

	public virtual void AutoStaticDefaults()
	{
		/*
		if (DisplayName.IsDefault())
			DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());
		*/
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
}
