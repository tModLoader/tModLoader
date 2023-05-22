namespace Terraria.ModLoader;

public interface IModType
{
	///<summary>
	/// The mod this belongs to.
	/// </summary>
	public Mod Mod { get; }

	/// <summary>
	/// The internal name of this instance.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// => $"{Mod.Name}/{Name}"
	/// </summary>
	public string FullName { get; } // default Properties when??
}
