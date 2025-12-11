using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Consumables
{
	// Food items have a unique item sprite for their eating and placed visuals. These are explained in the comments.
	public class ExampleFoodItem : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;

			// This is to show the correct frame in the inventory
			// The MaxValue argument is for the animation speed, we want it to be stuck on frame 1
			// Setting it to max value will cause it to take 414 days to reach the next frame
			// No one is going to have game open that long so this is fine
			// The second argument is the number of frames, which is 3
			// The first frame is the inventory texture, the second frame is the holding texture,
			// and the third frame is the placed texture
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

			// This allows you to change the color of the crumbs that are created when you eat.
			// The numbers are RGB (Red, Green, and Blue) values which range from 0 to 255.
			// Most foods have 3 crumb colors, but you can use more or less if you desire.
			// Depending on if you are making solid or liquid food switch out FoodParticleColors
			// with DrinkParticleColors. The difference is that food particles fly outwards
			// whereas drink particles fall straight down and are slightly transparent
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(249, 230, 136),
				new Color(152, 93, 95),
				new Color(174, 192, 192)
			];

			ItemID.Sets.IsFood[Type] = true; // This allows it to be placed on a plate and held correctly
		}

		public override void SetDefaults() {
			// This code matches the ApplePie code.

			// DefaultToFood sets all of the food related item defaults such as the buff type, buff duration, use sound, and animation time.
			Item.DefaultToFood(22, 22, BuffID.WellFed3, 57600); // 57600 is 16 minutes: 16 * 60 * 60
			Item.value = Item.buyPrice(0, 3);
			Item.rare = ItemRarityID.Blue;
		}

		// If you want multiple buffs, you can apply the remainder of buffs with this method.
		// Make sure the primary buff is set in SetDefaults so that the QuickBuff hotkey can work properly.
		public override void OnConsumeItem(Player player) {
			player.AddBuff(BuffID.SugarRush, 3600);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddIngredient(ItemID.Apple, 3)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}