#nullable enable
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		public static Generic Generic { get; private set; } = new Generic();
		public static NoScaling NoScaling { get; private set; } = new NoScaling();
		public static Melee Melee { get; private set; } = new Melee();
		public static Ranged Ranged { get; private set; } = new Ranged();
		public static Magic Magic { get; private set; } = new Magic();
		public static Summon Summon { get; private set; } = new Summon();
		public static Throwing Throwing { get; private set; } = new Throwing();

		internal int index;

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

		public override void Load() {
			Generic.index = 0;
			NoScaling.index = 1;
			Melee.index = 2;
			Ranged.index = 3;
			Magic.index = 4;
			Summon.index = 5;
			Throwing.index = 6;
		}

		protected override void Register() {
			index = DamageClassLoader.Add(this);

			ClassName = Mod.GetOrCreateTranslation($"Mods.{Mod.Name}.DamageClassName.{Name}");

			ModTypeLookup<DamageClass>.Register(this);
		}
	}
}
