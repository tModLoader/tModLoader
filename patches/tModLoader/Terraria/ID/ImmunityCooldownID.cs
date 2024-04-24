using System;

namespace Terraria.ID;

/// <summary>
/// Enumerates the different immunity cooldown options for damage affecting a player. Most damage uses <see cref="General"/> and applies immunity via <see cref="Player.immune"/>. Other damage immunity cooldowns are tracked in <see cref="Player.hurtCooldowns"/> indexed by these values.<para/>
/// Correct usage of <see cref="ImmunityCooldownID"/> in <see cref="ModLoader.ModProjectile.CooldownSlot"/>, <see cref="ModLoader.ModNPC.CanHitPlayer(Terraria.Player, ref int)"/>, and <see cref="Player.Hurt(DataStructures.PlayerDeathReason, int, int, bool, bool, int, bool, float, float, float)"/> are essential for correctly applying damage to the player.
/// </summary>
public static class ImmunityCooldownID
{
	/// <summary>
	/// Default, no special slot, just <see cref="Player.immuneTime"/>
	/// </summary>
	public const int General = -1;
	/// <summary>
	/// Contacting with tiles that deals damage, such as spikes and cactus in don't starve world
	/// </summary>
	public const int TileContactDamage = 0;
	/// <summary>
	/// Bosses like Moon Lord and Empress of Light (and their minions and projectiles)<para/>
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
