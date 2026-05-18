using Terraria.ModLoader;

namespace Terraria.ID;

/// <summary>
/// Values used to identify special banks ("personal storage") with the <see cref="Player.chest"></see> field.
/// </summary>
public static class BankID
{
	/// <summary>
	///	Represents &quot;no bank&quot;.
	/// </summary>
	public const int None = -1;

	/// <summary>
	/// For the corresponding items, see <see cref="Player.bank"/>.
	/// For the corresponding <see cref="PortableStorage"/>, see <see cref="PortableStorage.PiggyBank"/>.
	/// </summary>
	public const int PiggyBank = -2;

	/// <summary>
	/// For the corresponding items, see <see cref="Player.bank2"/>.
	/// For the corresponding <see cref="PortableStorage"/>, see <see cref="PortableStorage.Safe"/>.
	/// </summary>
	public const int Safe = -3;

	/// <summary>
	/// For the corresponding items, see <see cref="Player.bank3"/>.
	/// For the corresponding <see cref="PortableStorage"/>, see <see cref="PortableStorage.DefendersForge"/>.
	/// </summary>
	public const int DefendersForge = -4;

	/// <summary>
	/// For the corresponding items, see <see cref="Player.bank4"/>.
	/// For the corresponding <see cref="PortableStorage"/>, see <see cref="PortableStorage.VoidVault"/>.
	/// </summary>
	public const int VoidVault = -5;
}
