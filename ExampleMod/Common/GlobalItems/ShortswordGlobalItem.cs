using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	// This file shows a very simple example of a GlobalItem class. GlobalItem hooks are called on all items in the game and are suitable for sweeping changes like 
	// adding additional data to all items in the game. Here we simply adjust the damage of the Copper Shortsword item, as it is simple to understand. 
	// See other GlobalItem classes in ExampleMod to see other ways that GlobalItem can be used.
	public class ExampleGlobalItem : GlobalItem
	{
		// Here we make sure to only instance this GlobalItem for the Copper Shortsword, by checking item.type
		public override bool InstanceForEntity(Item item) {
			return item.type == ItemID.CopperShortsword;
		}

		public override void SetDefaults(Item item) {
			item.damage = 50; // Change damage to 50!
		}

		public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			// Make it shoot grenades for no reason
			Projectile.NewProjectileDirect(player.Center, new Vector2(speedX, speedY) * 5f, ProjectileID.Grenade, damage, knockBack, player.whoAmI);

			return true;
		}
	}
}
