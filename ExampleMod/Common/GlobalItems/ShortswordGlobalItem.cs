using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	// This file shows a very simple example of a GlobalItem class. GlobalItem hooks are called on all items in the game and are suitable for sweeping changes like
	// adding additional data to all items in the game. Here we simply adjust the damage of the Copper Shortsword item, as it is simple to understand.
	// See other GlobalItem classes in ExampleMod to see other ways that GlobalItem can be used.
	public class ShortswordGlobalItem : GlobalItem
	{
		// Here we make sure to only instance this GlobalItem for the Copper Shortsword, by checking item.type
		public override bool AppliesToEntity(Item item, bool lateInstantiation) {
			return item.type == ItemID.CopperShortsword;
		}

		public override void SetDefaults(Item item) {
			item.StatsModifiedBy.Add(Mod); // Notify the game that we've made a functional change to this item.

			item.damage = 50; // Change damage to 50!
		}

		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Make it shoot grenades for no reason
			Projectile.NewProjectileDirect(source, player.Center, velocity * 5f, ProjectileID.Grenade, damage, knockback, player.whoAmI);
			// Returning false prevents vanilla's shooting behavior from running.
			// In this case it prevents the shortsword's blade stabbing animation, as the blade itself is a projectile.
			return false;
		}
	}
}
