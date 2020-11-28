using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		public static Melee Melee => ModContent.GetInstance<Melee>();
		public static Ranged Ranged => ModContent.GetInstance<Ranged>();
		public static Magic Magic => ModContent.GetInstance<Magic>();
		public static Summon Summon => ModContent.GetInstance<Summon>();
		public static Throwing Throwing => ModContent.GetInstance<Throwing>();

		internal int index;

		/// <summary> This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part. </summary>
		public ModTranslation ClassName { get; internal set; }

		/// <summary> This is the name that will show up when an item tooltip displays 'X [ClassName]'. This should include the 'damage' part. </summary>
		public string DisplayName => DisplayNameInternal;

		internal protected virtual string DisplayNameInternal => ClassName.GetTranslation(Language.ActiveCulture);

		protected override void Register() {
			index = DamageClassLoader.Add(this);

			ClassName = Mod.GetOrCreateTranslation($"Mods.{Mod.Name}.DamageClassName.{Name}");

			ModTypeLookup<DamageClass>.Register(this);
		}
	}
}
