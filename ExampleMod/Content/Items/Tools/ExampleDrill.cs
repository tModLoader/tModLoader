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
			// ItemID.Sets.IsChainsaw[Type] = true;
			// IsDrill and IsChainsaw normally automatically reduce useTime and useAnimation to 60% of what is set in SetDefaults and decrease tileBoost by 1.
			// They will not decrease tileBoost by 2 or decrease use time to 36% if both are true.

			// Because modded item defaults are set after this takes place, you need to set useTime/useAnimation manually.
			// tileBoost will be set, but it will be replaced if you set it again in SetDefaults.
		}

		public override void SetDefaults() {
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.width = 20;
			Item.height = 12;
			// IsDrill/IsChainsaw effects must be applied manually, so 60% or 0.6 times the time of a pickaxe. In this case, 60% of 10, or 6.
			// If you decide to copy values from vanilla drills or chainsaws, you should multiply each one by 0.6 to get actual time.
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 0.6f;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item23;
			Item.shoot = ModContent.ProjectileType<ExampleDrillProjectile>(); //Create the drill projectile
			Item.shootSpeed = 32f;
			Item.noMelee = true; // Turns off damage from the item itself, as we have a projectile
			Item.noUseGraphic = true; // Stops the item from drawing in your hands, for the aforementioned reason
			Item.channel = true; // Important as the projectile checks if the player channels

			// tileBoost changes the range of tiles that the item can reach. Since IsDrill/IsChainsaw (above) is true, we can leave it alone...
			// Or we can give the drill 10 blocks of of extra range
			Item.tileBoost = 10;

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
