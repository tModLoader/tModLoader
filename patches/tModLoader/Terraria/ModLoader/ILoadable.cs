namespace Terraria.ModLoader
{
	/// <summary>
	/// Allows for implementing types to be loaded and unloaded.
	/// </summary>
	[Autoload]
	public interface ILoadable
	{
		/// <summary>
		/// Called when loading the type.
		/// </summary>
		/// <param name="mod">The mod instance associated with this type.</param>
		public abstract void Load(Mod mod);

		/// <summary>
		/// Called during unloading when needed.
		/// </summary>
		public abstract void Unload();
	}
}
