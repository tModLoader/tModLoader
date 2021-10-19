using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <inheritdoc cref="ModifyItemShootStats"/>
	[ComponentHook]
	public partial interface IModifyItemShootStatsHook
	{
		/// <summary>
		/// Allows you to modify the position, velocity, type, damage and/or knockback of a projectile being shot by any entity.
		/// </summary>
		/// <param name="player"> The owner of the item that's currently shooting. </param>
		/// <param name="item"> The item that's currently shooting. </param>
		/// <param name="position"> The center position of the projectile. </param>
		/// <param name="velocity"> The velocity of the projectile. </param>
		/// <param name="type"> The ID of the projectile. </param>
		/// <param name="damage"> The damage of the projectile. </param>
		/// <param name="knockback"> The knockback of the projectile. </param>
		void ModifyItemShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);

		/// <inheritdoc cref="ModifyItemShootStats"/>
		public static void Invoke(GameObject gameObject, Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				function(obj, player, item, ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}
	}
}
