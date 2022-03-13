﻿using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		public static DamageClass Default { get; private set; } = new DefaultDamageClass();
		public static DamageClass Generic { get; private set; } = new GenericDamageClass();
		public static DamageClass Melee { get; private set; } = new MeleeDamageClass();
		public static DamageClass Ranged { get; private set; } = new RangedDamageClass();
		public static DamageClass Magic { get; private set; } = new MagicDamageClass();
		public static DamageClass Summon { get; private set; } = new SummonDamageClass();
		public static DamageClass Throwing { get; private set; } = new ThrowingDamageClass();

		internal float[] statInheritanceCache;

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
		/// Returns 0 in all cases by default, which does not let any other classes boost this DamageClass.
		/// The return value works like a percentage; for example, 1f will allow your DamageClass to benefit from another class' stat bonuses at 100% effectiveness, while 0.5f will only allow that benefit at 50% effectiveness.
		/// For a more in-depth explanation and demonstration, refer to ExampleMod/Content/DamageClasses/ExampleDamageClass.
		/// </summary>
		/// <param name="damageClass">The DamageClass which you want this DamageClass to benefit from statistically.</param>
		protected virtual float CheckClassStatInheritance(DamageClass damageClass) => 0;

		internal void RebuildBenefitCache()
		{
			statInheritanceCache = DamageClassLoader.DamageClasses.Select(GetBenefitFrom).ToArray();
			statInheritanceCache[Type] = 1f;
		}

		public float CheckCachedClassStatInheritance(DamageClass damageClass) => statInheritanceCache[damageClass.Type];

		/// <summary> 
		/// This lets you define the classes that this DamageClass will count as (other than itself) for the purpose of armor and accessory effects, such as Spectre armor's bolts on magic attacks, or Magma Stone's Hellfire debuff on melee attacks.
		/// Returns false in all cases by default, which does not let any other classes' effects trigger on this DamageClass.
		/// For a more in-depth explanation and demonstration, refer to ExampleMod/Content/DamageClasses/ExampleDamageClass.
		/// </summary>
		/// <param name="damageClass">The DamageClass which you want this DamageClass to gain effects from.</param>
		public virtual bool CheckClassEffectInheritance(DamageClass damageClass) => false;

		/// <summary> 
		/// This lets you define default stat modifiers for all items of this class (e.g. base crit chance).
		/// </summary>
		public virtual void SetDefaultStats(Player player) { }

		/// <summary>
		/// This lets you decide whether or not your damage class can use standard crit chance calculations.
		/// Setting this to false will also hide the critical strike chance line in the tooltip of any item that uses this DamageClass.
		/// </summary>
		public virtual bool AllowStandardCrits => true;

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

		protected sealed override void Register()
		{
			ClassName = LocalizationLoader.GetOrCreateTranslation(Mod, $"DamageClassName.{Name}");

			ModTypeLookup<DamageClass>.Register(this);

			Type = DamageClassLoader.Add(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();
	}
}