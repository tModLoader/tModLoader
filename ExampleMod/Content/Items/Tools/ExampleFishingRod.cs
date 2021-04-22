using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	public class ExampleFishingRod : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Fishing Rod");
			Tooltip.SetDefault("Fires multiple lines at once. Can fish in lava.\n" +
				"The fishing line never snaps.");
			// Allows the pole to fish in lava
			ItemID.Sets.CanFishInLava[Item.type] = true;
		}

		public override void SetDefaults() {
			// These are copied through the CloneDefaults method:
			// Item.width = 24;
			// Item.height = 28;
			// Item.useStyle = ItemUseStyleID.Swing;
			// Item.useAnimation = 8;
			// Item.useTime = 8;
			// Item.UseSound = SoundID.Item1;
			Item.CloneDefaults(ItemID.WoodFishingPole);

			Item.fishingPole = 30; // Sets the poles fishing power		
			Item.shootSpeed = 12f; // Sets the speed in which the bobbers are launched. Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f.
			Item.shoot = ModContent.ProjectileType<Projectiles.ExampleBobber>(); // The Bobber projectile.
		}

		// Grants the High Test Fishing Line bool if holding the item.
		// NOTE: Only triggers through the hotbar, not if you hold the item by hand outside of the inventory.
		public override void HoldItem(Player player) {
			player.accFishingLine = true;
		}

		// Overrides the default shooting method to fire multiple bobbers.
		// NOTE: This will allow the fishing rod to summon multiple Duke Fishrons with multiple Truffle Worms in the inventory.
		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int bobberAmount = Main.rand.Next(3, 6); // 3 to 5 bobbers
			float spreadAmount = 75f; // how much the different bobbers are spread out.

			for (int index = 0; index < bobberAmount; ++index) {
				Vector2 bobberSpeed = velocity + new Vector2(Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f, Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f);

				// Generate new bobbers
				Projectile.NewProjectile(source, position, bobberSpeed, type, 0, 0f, player.whoAmI);
			}
			return false;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}