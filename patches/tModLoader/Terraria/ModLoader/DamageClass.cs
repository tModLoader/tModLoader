using System;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		/// <summary>
		/// Default damage class for non-classed weapons and items, does not benefit from Generic bonuses
		/// </summary>
		public static DamageClass Default { get; private set; } = new DefaultDamageClass();

		/// <summary>
		/// Base damage class for all weapons. All vanilla damage classes inherit bonuses applied to this class.
		/// Accessories which benefit all classes provide bonuses via the Generic class
		/// </summary>
		public static DamageClass Generic { get; private set; } = new GenericDamageClass();
		public static DamageClass Melee { get; private set; } = new MeleeDamageClass();
		public static DamageClass Ranged { get; private set; } = new RangedDamageClass();
		public static DamageClass Magic { get; private set; } = new MagicDamageClass();
		public static DamageClass Summon { get; private set; } = new SummonDamageClass();

		/// <summary>
		/// This is a damage class used solely by vanilla whips. It benefits from melee attackSpeed bonuses.
		/// </summary>
		public static DamageClass SummonMeleeSpeed { get; private set; } = new SummonMeleeSpeedDamageClass();

		/// <summary>
		/// This is a damage class used solely by vanilla forbidden storm. It scales with both magic and summon damage modifiers.
		/// </summary>
		public static DamageClass MagicSummonHybrid { get; private set; } = new MagicSummonHybridDamageClass();

		/// <summary>
		/// Class provided for modders who want to coordinate throwing accessories and items. Not used by any vanilla items.
		/// </summary>
		public static DamageClass Throwing { get; private set; } = new ThrowingDamageClass();


		/// <summary>
		/// This is the internal ID of this DamageClass.
		/// </summary>
		public int Type { get; internal set; }

		/// <summary>
		/// This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part.
		/// </summary>
		public ModTranslation ClassName { get; internal set; }

		/// <summary>
		/// This is the name that will show up when an item tooltip displays 'X [ClassName]'.
		/// This should include the 'damage' part.
		/// </summary>
		public string DisplayName => DisplayNameInternal;

		internal protected virtual string DisplayNameInternal => ClassName.GetTranslation(Language.ActiveCulture);

		/// <summary>
		/// This lets you define the classes that this DamageClass will benefit from (other than itself) for the purposes of stat bonuses, such as damage and crit chance.
		/// This returns a struct called StatInheritanceData.
		/// This is used to allow extensive specifications for what your damage class can and can't benefit from in terms of other classes' stat bonuses.
		/// By default, this will return StatInheritanceData.Full for DamageClass.Generic, and StatInheritanceData.None for any other.
		/// Any given DamageClass will also never call this method for itself.
		/// For a more in-depth explanation and demonstration, refer to ExampleMod/Content/DamageClasses/ExampleDamageClass.
		/// </summary>
		/// <param name="damageClass">The DamageClass which you want this DamageClass to benefit from statistically.</param>
		public virtual StatInheritanceData GetModifierInheritance(DamageClass damageClass) => damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;

		/// <summary> 
		/// This lets you define the classes that this DamageClass will count as (other than itself) for the purpose of armor and accessory effects, such as Spectre armor's bolts on magic attacks, or Magma Stone's Hellfire debuff on melee attacks.
		/// Returns false in all cases by default, which does not let any other classes' effects trigger on this DamageClass.
		/// For a more in-depth explanation and demonstration, refer to ExampleMod/Content/DamageClasses/ExampleDamageClass.
		/// </summary>
		/// <param name="damageClass">The DamageClass which you want this DamageClass to gain effects from.</param>
		public virtual bool GetEffectInheritance(DamageClass damageClass) => false;

		/// <summary> 
		/// This lets you define default stat modifiers for all items of this class (e.g. base crit chance).
		/// </summary>
		public virtual void SetDefaultStats(Player player) { }

		/// <summary>
		/// This lets you decide whether or not your damage class will use standard crit chance calculations.
		/// Setting this to false will also hide the critical strike chance line in the tooltip of any item that uses this DamageClass.
		/// </summary>
		public virtual bool UseStandardCritCalcs => true;

		// to-do:
		// this is a horrible approach to doin' this and I know full well that it is; this is just a stopgap to simplify the process for now
		// once the tooltip rework happens, in-depth explanations for things like this, with examples primarily as a supplement to the lesson, NEED to be made available
		// that way, we can strike this in favor of actually havin' good learnin' resources
		// - thomas
		/// <summary>
		/// This lets you enable or disable standard statistical tooltip lines displaying on items associated with this DamageClass.
		/// The lines usable are "Damage", "CritChance", "Speed", and "Knockback".
		/// </summary>
		public virtual bool ShowStatTooltipLine(Player player, string lineName) => true;

		protected sealed override void Register() {
			ClassName = LocalizationLoader.GetOrCreateTranslation(Mod, $"DamageClassName.{Name}");

			ModTypeLookup<DamageClass>.Register(this);

			Type = DamageClassLoader.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();
	}
}