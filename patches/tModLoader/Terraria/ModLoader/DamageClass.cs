using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		public static DamageClass Generic => ModContent.GetInstance<GenericDamageClass>();
		public static DamageClass NoScaling => ModContent.GetInstance<NoScalingDamageClass>();
		public static DamageClass Melee => ModContent.GetInstance<MeleeDamageClass>();
		public static DamageClass Ranged => ModContent.GetInstance<RangedDamageClass>();
		public static DamageClass Magic => ModContent.GetInstance<MagicDamageClass>();
		public static DamageClass Summon => ModContent.GetInstance<SummonDamageClass>();
		public static DamageClass Throwing => ModContent.GetInstance<ThrowingDamageClass>();

		internal float[] benefitsCache;

		/// <summary>
		/// This is the internal ID of this DamageClass.
		/// </summary>
		public int Type { get; internal set; }

		/// <summary>
		/// This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part.
		/// </summary>
		public ModTranslation ClassName { get; internal set; }

		/// <summary> This is the name that will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part. </summary>
		public string DisplayName => DisplayNameInternal;

		internal protected virtual string DisplayNameInternal => ClassName.GetTranslation(Language.ActiveCulture);

		/// <summary>
		/// This lets you define the classes that this DamageClass will benefit from (other than itself) for the purposes of stat bonuses, such as damage and crit chance.
		/// Returns 0 in all cases by default, which does not let any other classes boost this DamageClass.
		/// </summary>
		/// <param name="damageClass">The DamageClass which you want this DamageClass to benefit from statistically.</param>
		protected virtual float GetBenefitFrom(DamageClass damageClass) => 0;

		internal void RebuildBenefitCache() {
			benefitsCache = DamageClassLoader.DamageClasses.Select(GetBenefitFrom).ToArray();
			benefitsCache[Type] = 1f;
		}

		public float GetCachedBenefitFrom(DamageClass damageClass) => benefitsCache[damageClass.Type];

		/// <summary> 
		/// This lets you define the classes that this DamageClass will count as (other than itself) for the purpose of armor and accessory effects, such as Spectre armor's bolts on magic attacks, or Magma Stone's Hellfire debuff on melee attacks.
		/// Returns false in all cases by default, which does not let any other classes' effects trigger on this DamageClass.
		/// </summary>
		/// <param name="damageClass">The DamageClass which you want this DamageClass to gain effects from.</param>
		public virtual bool CountsAs(DamageClass damageClass) => false;

		/// <summary> 
		/// This lets you define default buffs for all items of this class (eg, base crit)
		/// </summary>
		public virtual void SetDefaultStats(Player player) {}

		protected override void Register() {
			ClassName = Mod.GetOrCreateTranslation($"Mods.{Mod.Name}.DamageClassName.{Name}");

			ModTypeLookup<DamageClass>.Register(this);

			Type = DamageClassLoader.Add(this);
		}
	}
}
