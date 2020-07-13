namespace Terraria.ModLoader
{
	/// <summary>
	/// Allows for implementing types to be autoloaded.
	/// </summary>
	public interface IAutoloadable
	{
		/// <summary>
		/// Called when loading the type.
		/// </summary>
		/// <param name="mod">The mod instance associated with this type.</param>
		/// <returns>True is this type needs to be unloaded, otherwise false.</returns>
		bool Autoload(Mod mod);

		/// <summary>
		/// Called during unloading when needed.
		/// </summary>
		void Unload();
	}
}
