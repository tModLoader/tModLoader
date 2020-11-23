#nullable enable
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		public static Generic Generic => ModContent.GetInstance<Generic>();
		public static NoScaling NoScaling => ModContent.GetInstance<NoScaling>();
		public static Melee Melee => ModContent.GetInstance<Melee>();
		public static Ranged Ranged => ModContent.GetInstance<Ranged>();
		public static Magic Magic => ModContent.GetInstance<Magic>();
		public static Summon Summon => ModContent.GetInstance<Summon>();
		public static Throwing Throwing => ModContent.GetInstance<Throwing>();

		/// <summary> This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part. </summary>
		public ModTranslation ClassName { get; internal set; }

		/// <summary> This is the name that will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part. </summary>
		public string DisplayName => DisplayNameInternal;
		/// <summary>
		/// This defines the classes that this DamageClass will benefit from (other than itself) for the purposes of stat bonuses, such as damage and crit chance.
		/// Returns null by default, which does not let any other classes boost this DamageClass.
		/// </summary>
		public virtual Dictionary<DamageClass, float>? BenefitsFrom() {
			return null;
		}

		/// <summary> 
		/// This defines the classes that this DamageClass will count as (other than itself) for the purpose of armor and accessory effects, such as Spectre armor's bolts on magic attacks, or Magma Stone's Hellfire debuff on melee attacks.
		/// Returns null by default, which does not let any other classes' effects trigger on this DamageClass.
		/// </summary>
		public virtual List<DamageClass>? CountsAs() {
			return null;
		}
		internal protected virtual string DisplayNameInternal => ClassName.GetTranslation(Language.ActiveCulture);

		protected override void Register() {
			DamageClassLoader.Add(this);

			ClassName = Mod.GetOrCreateTranslation($"Mods.{Mod.Name}.DamageClassName.{Name}");

			ModTypeLookup<DamageClass>.Register(this);
		}
	}
}
