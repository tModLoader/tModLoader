using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// This example shows how to create a boomerang weapon that limits the player to one active boomerang at a time.
	// See ExampleBoomerangProjectile for custom boomerang AI and an alternative that reuses vanilla behavior.
	public class ExampleBoomerang : ModItem
	{
		public override void SetDefaults() {
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
			// Vanilla limits its basic boomerangs by projectile type. Modded boomerangs need their own check.
			// Only allow another throw once this player's previous boomerang is no longer active.
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
