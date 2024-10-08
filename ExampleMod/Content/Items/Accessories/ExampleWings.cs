using ExampleMod.Common.Configs;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Wings)]
	public class ExampleWings : ModItem
	{
		// To see how this config option was added, see ExampleModConfig.cs
		// This code allows users to toggle loading this content via a config. Another common usage of IsLoadingEnabled would be to use ModLoader.HasMod to check if another mod is enabled or not.
		// Feel free to remove this method in your own Wings if using this as a template, it is superfluous.
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<ExampleModConfig>().ExampleWingsToggle;
		}

		public override void SetStaticDefaults() {
			// These wings use the same values as the solar wings
			// Fly time: 180 ticks = 3 seconds
			// Fly speed: 9
			// Acceleration multiplier: 2.5
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 20;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.accessory = true;
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f; // Falling glide speed
			ascentWhenRising = 0.15f; // Rising speed
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.SortBefore(Main.recipe.First(recipe => recipe.createItem.wingSlot != -1)) // Places this recipe before any wing so every wing stays together in the crafting menu.
				.Register();
		}

		public override bool ModifyEquipTextureDraw(ref PlayerDrawSet drawInfo, ref DrawData drawData, EquipType type, int slot, string memberName) {

			drawData.sourceRect = new(0, drawData.texture.Height / 7 * drawInfo.drawPlayer.wingFrame, drawData.texture.Width, drawData.texture.Height / 7);

			if (drawInfo.drawPlayer.direction == 1)
				drawData.position += Main.rand.NextVector2CircularEdge(10, 10);

			return true;
		}

		public override bool WingUpdate(Player player, bool inUse) {
			if (inUse || player.jump > 0) {
				player.wingFrameCounter++;
				if (player.wingFrameCounter > 3) {
					player.wingFrame++;
					player.wingFrameCounter = 0;
					if (player.wingFrame >= 7)
						player.wingFrame = 1;
				}
			}
			else if (player.velocity.Y != 0f) {
				player.wingFrame = 2;
				if (player.ShouldFloatInWater && player.wet)
					player.wingFrame = 0;
			}
			else {
				player.wingFrame = 0;
			}

			if (!inUse && player.wingsLogic > 0 && player.controlJump && player.velocity.Y > 0f) {
				player.wingFrame = 1;
			}

			// Returning true to skip vanilla animations
			return true;
		}
	}
}
