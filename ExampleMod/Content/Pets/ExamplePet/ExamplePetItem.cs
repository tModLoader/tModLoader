using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Pets.ExamplePet
{
	public class ExamplePetItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paper Airplane");
			Tooltip.SetDefault("Summons a Paper Airplane to follow aimlessly behind you");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.ZephyrFish); // Copy the Defaults of the Zephyr Fish item.

			item.shoot = ProjectileType<ExamplePetProjectile>(); // "Shoot" your pet projectile.
			item.buffType = BuffType<ExamplePetBuff>(); // Apply buff upon usage of the item.
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}

		public override void UseStyle(Player player) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(item.buffType, 3600);
			}
		}
	}
}
