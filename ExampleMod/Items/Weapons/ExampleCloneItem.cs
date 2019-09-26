using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Weapons
{
	public class ExampleCloneItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Starfury V2");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.Starfury);
			item.shootSpeed *= 0.75f;
			item.damage = (int)(item.damage * 1.5);
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			type = ProjectileType<ExampleCloneProjectile>();
			return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Starfury);
			recipe.AddRecipeGroup("IronBar", 5);
			recipe.AddTile(TileID.FireflyinaBottle);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}