namespace Terraria.ID;

/// <summary>
/// Values used to identify special banks ("personal storage") with the <see cref="Player.chest"></see> field.
/// </summary>
public static class BankID
{
	public const int None = -1;
	/// <summary> For the corresponding items, see <see cref="Player.bank"/> </summary>
	public const int PiggyBank = -2;
	/// <summary> For the corresponding items, see <see cref="Player.bank2"/> </summary>
	public const int Safe = -3;
	/// <summary> For the corresponding items, see <see cref="Player.bank3"/> </summary>
	public const int DefendersForge = -4;
	/// <summary> For the corresponding items, see <see cref="Player.bank4"/> </summary>
	public const int VoidVault = -5;
}
