namespace Terraria.ModLoader
{
	public abstract class DamageClass : ModType
	{
		internal int index;

		/// <summary>
		/// This is the name that will show up when an item tooltip displays 'X [name] damage'.
		/// </summary>
		public virtual string ClassName => Name;

		protected override void Register() {
			index = DamageClassLoader.Add(this);

			ModTypeLookup<DamageClass>.Register(this);
		}

		public static Melee Melee { get; private set; } = new Melee();
		public static Ranged Ranged { get; private set; } = new Ranged();
		public static Magic Magic { get; private set; } = new Magic();
		public static Summon Summon { get; private set; } = new Summon();
	}
}
