using ExampleMod.Content.Items.Placeable.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleBlock : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded tile.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
			ItemID.Sets.ExtractinatorMode[Item.type] = Item.type;

			// Some please convert this to lang files, I'm too lazy to do it
			// Sorry Itorius, I feel you

			// DisplayName.AddTranslation(GameCulture.German, "Beispielblock");
			// Tooltip.AddTranslation(GameCulture.German, "Dies ist ein modded Block");
			// DisplayName.AddTranslation(GameCulture.Italian, "Blocco di esempio");
			// Tooltip.AddTranslation(GameCulture.Italian, "Questo è un blocco moddato");
			// DisplayName.AddTranslation(GameCulture.French, "Bloc d'exemple");
			// Tooltip.AddTranslation(GameCulture.French, "C'est un bloc modgé");
			// DisplayName.AddTranslation(GameCulture.Spanish, "Bloque de ejemplo");
			// Tooltip.AddTranslation(GameCulture.Spanish, "Este es un bloque modded");
			// DisplayName.AddTranslation(GameCulture.Russian, "Блок примера");
			// Tooltip.AddTranslation(GameCulture.Russian, "Это модифицированный блок");
			// DisplayName.AddTranslation(GameCulture.Chinese, "例子块");
			// Tooltip.AddTranslation(GameCulture.Chinese, "这是一个修改块");
			// DisplayName.AddTranslation(GameCulture.Portuguese, "Bloco de exemplo");
			// Tooltip.AddTranslation(GameCulture.Portuguese, "Este é um bloco modded");
			// DisplayName.AddTranslation(GameCulture.Polish, "Przykładowy blok");
			// Tooltip.AddTranslation(GameCulture.Polish, "Jest to modded blok");
		}

		public override void SetDefaults() {
			Item.width = 12;
			Item.height = 12;
			Item.maxStack = 999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<Tiles.ExampleBlock>();
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(10)
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();

			CreateRecipe() // Add multiple recipes set to one Item.
				.AddIngredient<ExampleWall>(4)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();

			CreateRecipe()
				.AddIngredient<ExamplePlatform>(2)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		public override void ExtractinatorUse(ref int resultType, ref int resultStack) { // Calls upon use of an extractinator. Below is the chance you will get ExampleOre from the extractinator.
			if (Main.rand.NextBool(3)) {
				resultType = ModContent.ItemType<ExampleOre>();  // Get this from the extractinator with a 1 in 3 chance.
				if (Main.rand.NextBool(5)) {
					resultStack += Main.rand.Next(2); // Add a chance to get more than one of ExampleOre from the extractinator.
				}
			}
		}
	}
}
