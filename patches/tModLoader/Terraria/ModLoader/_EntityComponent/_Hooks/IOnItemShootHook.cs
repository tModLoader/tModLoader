using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	/// <inheritdoc cref="OnItemShoot"/>
	[ComponentHook]
	public partial interface IOnItemShootHook
	{
		/// <summary>
		/// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by any entity.
		/// </summary>
		/// <param name="player"> The user of the entity that's currently shooting. This may have a player or an npc component. </param>
		/// <param name="item"> The entity that's currently shooting. This may have an item component. </param>
		/// <param name="position"> The center position of the projectile. </param>
		/// <param name="velocity"> The velocity of the projectile. </param>
		/// <param name="type"> The ID of the projectile. </param>
		/// <param name="damage"> The damage of the projectile. </param>
		/// <param name="knockback"> The knockback of the projectile. </param>
		void OnItemShoot(Player player, Item item, ProjectileSource_Item_WithAmmo projectileSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback);

		/// <inheritdoc cref="OnItemShoot"/>
		public static void Invoke(GameObject gameObject, Player player, Item item, ProjectileSource_Item_WithAmmo projectileSource, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				function(obj, player, item, projectileSource, position, velocity, type, damage, knockback);
			}
		}
	}
}
