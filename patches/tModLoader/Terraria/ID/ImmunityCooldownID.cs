namespace Terraria.ID;

public static partial class ImmunityCooldownID
{
	/// <summary>
	/// Default, no special slot, just Player.immuneTime
	/// </summary>
	public const int General = -1;
	/// <summary>
	/// Contacting with tiles that deals damage, such as spikes and cactus in don't starve world
	/// </summary>
	public const int TileContactDamage = 0;
	/// <summary>
	/// Bosses like Moon Lord and Empress of Light (and their minions and projectiles)
	/// Prevents cheesing by taking repeated low damage from another source
	/// </summary>
	public const int Bosses = 1;
	public const int DD2OgreKnockback = 2;
	/// <summary>
	/// Trying to catch lava critters with regular bug net
	/// </summary>
	public const int WrongBugNet = 3;
	/// <summary>
	/// Damage from lava
	/// </summary>
	public const int Lava = 4;
}
