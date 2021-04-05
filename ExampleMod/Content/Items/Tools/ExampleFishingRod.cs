using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleFishingRod : ModItem
	{
		// You can use vanilla textures by using the format: Terraria/Item_<ID>
		public override string Texture => "Terraria/Images/Item_" + ItemID.WoodFishingPole;
		public Color OverrideColor = Color.Coral;

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
			Item.color = OverrideColor; // Change the item's draw color so that it is visually distinct from the vanilla Wooden Fishing Rod.
		}

		// Grants the High Test Fishing Line bool if holding the item.
		// NOTE: Only triggers through the hotbar, not if you hold the item by hand outside of the inventory.
		public override void HoldItem(Player player) {
			player.accFishingLine = true;
		}

		// Overrides the default shooting method to fire multiple bobbers.
		// NOTE: This will allow the fishing rod to summon multiple Duke Fishrons with multiple Truffle Worms in the inventory.
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			int bobberAmount = Main.rand.Next(3, 6); // 3 to 5 bobbers
			float spreadAmount = 75f; // how much the different bobbers are spread out.

			for (int index = 0; index < bobberAmount; ++index) {
				float bobberSpeedX = speedX + Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f;
				float bobberSpeedY = speedY + Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f;

				// Generate new bobbers
				Projectile.NewProjectile(player.GetProjectileSource_Item(Item), position.X, position.Y, bobberSpeedX, bobberSpeedY, type, 0, 0f, player.whoAmI, 0f, 0f);
			}
			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<ExampleItem>(), 10) // This items needs 10 ExampleItems to craft
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // You need to craft this item on a ExampleWorkbench
				.Register();
		}
	}
}