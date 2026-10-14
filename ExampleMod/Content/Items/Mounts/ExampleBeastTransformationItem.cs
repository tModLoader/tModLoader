using ExampleMod.Content.Mounts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Mounts
{
	public class ExampleBeastTransformationItem : ModItem
	{
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 28;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp; // how the player's arm moves when using the item
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item25; // What sound should play when using the item
			Item.noMelee = true; // this item doesn't do any melee damage
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.mountType = ModContent.MountType<ExampleBeastTransformation>();
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddIngredient(ItemID.Bone)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
