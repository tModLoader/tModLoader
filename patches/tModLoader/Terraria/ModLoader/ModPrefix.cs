using System.Text.RegularExpressions;

namespace Terraria.ModLoader
{
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

	public abstract class ModPrefix : ModType
	{
		public int Type { get; internal set; }

		public ModTranslation DisplayName { get; internal set; }

		/// <summary>
		/// The category your prefix belongs to, PrefixCategory.Custom by default
		/// </summary>
		public virtual PrefixCategory Category => PrefixCategory.Custom;

		protected sealed override void Register() {
			ModTypeLookup<ModPrefix>.Register(this);

			Type = PrefixLoader.ReservePrefixID();
			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"Prefix.{Name}");

			PrefixLoader.RegisterPrefix(this);
		}

		public sealed override void SetupContent() {
			AutoDefaults();
			SetDefaults();
		}

		public virtual void AutoDefaults() {
			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(Name, "([A-Z])", " $1").Trim());
		}

		/// <summary>
		/// Allows you to set the prefix's name/translations and to set its category.
		/// </summary>
		public virtual void SetDefaults() { }

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
		/// Validates whether this prefix with the custom data stats set from SetStats is allowed on the given item.
		/// It is not allowed if one of the stat changes do not cause any change (eg. percentage being too small to make a difference).
		/// </summary>
		public virtual void ValidateItem(Item item, ref bool invalid) { }

		/// <summary>
		/// Applies the custom data stats set in SetStats to the given item.
		/// </summary>
		public virtual void Apply(Item item) { }

		/// <summary>
		/// Allows you to modify the sell price of the item based on the prefix or changes in custom data stats. This also influences the item's rarity.
		/// </summary>
		public virtual void ModifyValue(ref float valueMult) { }
	}
}
