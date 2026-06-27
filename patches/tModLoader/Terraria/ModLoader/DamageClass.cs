using System;
using System.Linq;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// <see cref="DamageClass"/> is used to determine the application of item effects, damage/stat scaling, and class bonuses.
/// </summary>
/// <remarks>
/// New classes can be created and can be set to inherit these applications from other classes.
/// <para>For a more in-depth explanation and demonstration refer to <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/DamageClasses/ExampleDamageClass.cs">ExampleMod's ExampleDamageClass.cs</see>.</para>
/// </remarks>
public abstract class DamageClass : ModType, ILocalizedModType
{
	public class Sets
	{
		/// <summary>
		/// Used for creating sets indexed by DamageClass type (<see cref="DamageClass.Type"/>).
		/// <para/> <inheritdoc cref="SetFactory"/>
		/// </summary>
		public static SetFactory Factory = new SetFactory(DamageClassLoader.DamageClassCount, nameof(DamageClass), Search);
	}
	public static IdDictionary Search = IdDictionary.Create<DamageClass, int>();

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

	/// <summary>
	/// This is a damage class used by various projectile-only vanilla melee weapons. Attack speed has no effect on items with this damage class.
	/// </summary>
	public static DamageClass MeleeNoSpeed { get; private set; } = new MeleeNoSpeedDamageClass();
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
	/// The internal ID of this <see cref="DamageClass"/>.
	/// </summary>
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "DamageClasses";

	/// <summary>
	/// This is the name that will show up when an item tooltip displays 'X [ClassName]'.
	/// </summary>
	/// <remarks>
	/// This should include the 'damage' part.
	/// Note that vanilla entries all start with a space that will need to be trimmed if used in other contexts.
	/// </remarks>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <summary>
	/// This lets you define the classes that this <see cref="DamageClass"/> will benefit from (other than itself) for the purposes of stat bonuses, such as damage and crit chance.
	/// This is used to allow extensive specifications for what your damage class can and can't benefit from in terms of other classes' stat bonuses.
	/// </summary>
	/// <param name="damageClass">The <see cref="DamageClass"/> which you want this <see cref="DamageClass"/> to benefit from statistically.</param>
	/// <returns>By default this will return <see cref="StatInheritanceData.Full"/> for <see cref="DamageClass.Generic"/> and <see cref="StatInheritanceData.None"/> for any other.</returns>
	public virtual StatInheritanceData GetModifierInheritance(DamageClass damageClass) => damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;

	/// <summary>
	/// This lets you define the classes that this <see cref="DamageClass"/> will count as (other than itself) for the purpose of armor and accessory effects, such as Spectre armor's bolts on magic attacks, or Magma Stone's Hellfire debuff on melee attacks.<br/>
	/// For a more in-depth explanation and demonstration, see <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Content/DamageClasses/ExampleDamageClass.cs">ExampleMod's ExampleDamageClass.cs</see>
	/// This method is only meant to be overridden. Modders should call <see cref="CountsAsClass"/> to query effect inheritance.
	/// </summary>
	/// <remarks>Return <see langword="true"/> for each <see cref="DamageClass"/> you want to inherit from</remarks>
	/// <param name="damageClass">The <see cref="DamageClass"/> you want to inherit effects from.</param>
	/// <returns><see langword="false"/> by default - which does not let any other classes' effects trigger on this <see cref="DamageClass"/>.</returns>
	public virtual bool GetEffectInheritance(DamageClass damageClass) => false;

	/// <summary>
	/// This lets you define the classes that this <see cref="DamageClass"/> will count as (other than itself) for the purpose of prefixes.<br/>
	/// This method is only meant to be overridden. Modders should call <see cref="GetsPrefixesFor"/> to query prefix inheritance.
	/// </summary>
	/// <remarks>Return <see langword="true"/> for each <see cref="DamageClass"/> you want to inherit from</remarks>
	/// <param name="damageClass">The <see cref="DamageClass"/> you want to inherit prefixes from.</param>
	/// <returns><see cref="GetEffectInheritance"/> by default - which lets the prefixes of any class this class inherits effects from roll and remain on items of this <see cref="DamageClass"/>.</returns>
	public virtual bool GetPrefixInheritance(DamageClass damageClass) => GetEffectInheritance(damageClass);

	/// <summary>
	/// This lets you define default stat modifiers for all items of this class (e.g. base crit chance).
	/// </summary>
	/// <param name="player">The player to apply stat modifications to</param>
	public virtual void SetDefaultStats(Player player) { }

	/// <summary>
	/// This lets you decide whether or not your damage class will use standard crit chance calculations.
	/// Setting this to <see langword="false"/> will also hide the critical strike chance line in the tooltip of any item that uses this <see cref="DamageClass"/>.
	/// </summary>
	public virtual bool UseStandardCritCalcs => true;



	// to-do:
	// this is a horrible approach to doin' this and I know full well that it is; this is just a stopgap to simplify the process for now
	// once the tooltip rework happens, in-depth explanations for things like this, with examples primarily as a supplement to the lesson, NEED to be made available
	// that way, we can strike this in favor of actually havin' good learnin' resources
	// - thomas
	/// <summary>
	/// Overriding this lets you disable standard statistical tooltip lines displayed on items associated with this <see cref="DamageClass"/>. All tooltip lines are enabled by default.
	/// </summary>
	/// <remarks>To disable tooltip lines you should return <see langword="false"/> for each of those cases.</remarks>
	/// <param name="player">The player to apply tooltip changes to</param>
	/// <param name="lineName">The tooltip line to change visibility for. Usable values are: "Damage", "CritChance", "Speed", and "Knockback"</param>
	public virtual bool ShowStatTooltipLine(Player player, string lineName) => true;

	protected sealed override void Register()
	{
		ModTypeLookup<DamageClass>.Register(this);
		Type = DamageClassLoader.Add(this);
		Search.Add(FullName, Type);
	}

	public sealed override void SetupContent()
	{
		_ = DisplayName;
		SetStaticDefaults();
	}

	/// <inheritdoc cref="CountsAsClass"/>
	public bool CountsAsClass<T>() where T : DamageClass
		=> CountsAsClass(ModContent.GetInstance<T>());

	/// <summary>
	/// This is used to check if this <see cref="DamageClass"/> has been set to inherit effects from the provided <see cref="DamageClass"/>, as dictated by <see cref="GetEffectInheritance"/>
	/// </summary>
	/// <param name="damageClass">The DamageClass you want to check if effects are inherited by this DamageClass.</param>
	/// <returns><see langword="true"/> if this damage class is inheriting effects from <paramref name="damageClass"/>, <see langword="false"/> otherwise</returns>
	public bool CountsAsClass(DamageClass damageClass)
		=> DamageClassLoader.effectInheritanceCache[Type, damageClass.Type];

	/// <inheritdoc cref="GetsPrefixesFor"/>
	public bool GetsPrefixesFor<T>() where T : DamageClass
		=> GetsPrefixesFor(ModContent.GetInstance<T>());

	/// <summary>
	/// This is used to check if this <see cref="DamageClass"/> has been set to inherit prefixes from the provided <see cref="DamageClass"/>, as dictated by <see cref="GetPrefixInheritance"/>
	/// </summary>
	/// <param name="damageClass">The DamageClass you want to check if prefixes are inherited by this DamageClass.</param>
	/// <returns><see langword="true"/> if this damage class inherits prefixes from <paramref name="damageClass"/>, <see langword="false"/> otherwise</returns>
	public bool GetsPrefixesFor(DamageClass damageClass)
		=> this == damageClass || GetPrefixInheritance(damageClass);
}
