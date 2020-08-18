namespace Terraria.ModLoader
{
	public abstract class ModDamageClass : ModType
	{
		public int Type { get; internal set; }

		/// <summary>
		/// This is the name that will show up when an item tooltip displays 'X [name] damage'.
		/// </summary>
		public virtual string ClassName => "";

		protected override void Register() {
			Mod.damageClasses[Name] = this;
			Type = DamageClassLoader.Add(this);
			ContentInstance.Register(this);
		}
	}
}
