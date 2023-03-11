namespace Terraria.ModLoader;

public interface IModType : ILoadable
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

	/// <summary>
	/// Array of internal names that this instance should alias as, useful for backwards compatibility after a refactor.
	/// <br/>Also see <seealso cref="LegacyNameAttribute"/> which may be more convenient.
	/// <br/>To get legacy names specified by this array and by the attribute, use <see cref="ModTypeLookup{T}.GetLegacyNames"/>.
	/// <br/>Should return an empty array by default.
	/// </summary>
	public string[] LegacyNames { get; } // default Properties when??
}
