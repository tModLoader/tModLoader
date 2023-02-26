using ExampleMod.Content.Dusts;
using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleDrill : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.IsDrill[Type] = true;
		}

		public override void SetDefaults() {
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.width = 20;
			Item.height = 12;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 0.6f;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item23;
			Item.shoot = ModContent.ProjectileType<ExampleDrillProjectile>();
			Item.shootSpeed = 32f;
			Item.noMelee = true; //Turns off damage from the item itself, as we have a projectile
			Item.noUseGraphic = true; //Stops the item from drawing in your hands, for the aforementioned reason
			Item.channel = true; //Important as the projectile checks if the player channels

			Item.pick = 220; // How strong the drill is, see https://terraria.wiki.gg/wiki/Pickaxe_power for a list of common values
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
