using ReLogic.Reflection;

namespace Terraria.ID;

/// <summary>
/// Corresponds to values assigned to <see cref="Player.dashType"/>.
/// </summary>
public class DashID
{
	public const int None = 0;
	public const int TabiAndMasterNinjaGear = 1;
	public const int ShieldOfCthulhu = 2;
	public const int SolarFlare = 3;
	public const int Unused4 = 4; // included for parity, code is in vanilla but not used.
	public const int CrystalAssassin = 5;
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(DashID), typeof(int)); // TML
}
