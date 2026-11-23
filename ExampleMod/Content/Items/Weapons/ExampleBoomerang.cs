using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// This example shows how to create a boomerang weapon.
	// Boomerang weapons typically limit the player to one active boomerang projectile at a time (See CanUseItem below).
	// See ExampleBoomerangProjectile for custom boomerang AI and an alternative that reuses vanilla behavior.
	public class ExampleBoomerang : ModItem
	{
		public override void SetDefaults() {
			// These defaults match Wooden Boomerang
			Item.width = 14;
			Item.height = 28;
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 5f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item1;
			Item.shoot = ModContent.ProjectileType<ExampleBoomerangProjectile>();
			Item.shootSpeed = 6.5f;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}

		public override bool CanUseItem(Player player) {
			// Boomerangs typically limit the player to a single boomerang projectile.
			// This check prevents the player from throwing another boomerang until the previous boomerang returns.
			return player.ownedProjectileCounts[Item.shoot] < 1;

			// Note: Some boomerangs, such as the Light Disc, allow multiple boomerangs at once.
			// To do this, change 1 in the check above to 5. Item.autoReuse should also be set to true in SetDefaults to match Light Disc behavior.
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
