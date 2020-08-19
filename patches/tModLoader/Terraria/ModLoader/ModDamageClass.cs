namespace Terraria.ModLoader
{
	public abstract class ModDamageClass : ModType
	{
		/// <summary>
		/// This is the name that will show up when an item tooltip displays 'X [name] damage'.
		/// </summary>
		public virtual string ClassName => "";

		protected override void Register() {
			Mod.damageClasses[Name] = this;
			DamageClassLoader.Add(this);
			ContentInstance.Register(this);
		}
	}
}
