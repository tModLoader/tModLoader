namespace Terraria.ModLoader
{
	/// <summary>
	/// The base type for most modded things.
	/// </summary>
	public abstract class ModType : IModType
	{
		///<summary>
		/// The mod this belongs to.
		/// </summary>
		public Mod Mod { get; internal set; }

		/// <summary>
		/// The internal name of this.
		/// </summary>
		public virtual string Name => GetType().Name;

		/// <summary>Make sure to call base.Mod(mod) to load in your type.</summary>
		public virtual void Load(Mod mod) => Mod = mod;

		public virtual void Unload(){}
	}

}
