using ExampleMod.Content.Items.Placeable.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleBlock : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.ExtractinatorMode[Type] = Item.type;

			// Mods can be translated to any of the languages tModLoader supports. See https://github.com/tModLoader/tModLoader/wiki/Localization
			// Translations go in localization files (.hjson files), but these are listed here as an example to help modders become aware of the possibility that users might want to use your mod in other languages:
			// English: "Example Block", "This is a modded tile."
			// German: "Beispielblock", "Dies ist ein modded Block"
			// Italian: "Blocco di esempio", "Questo è un blocco moddato"
			// French: "Bloc d'exemple", "C'est un bloc modgé"
			// Spanish: "Bloque de ejemplo", "Este es un bloque modded"
			// Russian: "Блок примера", "Это модифицированный блок"
			// Chinese: "例子块", "这是一个修改块"
			// Portuguese: "Bloco de exemplo", "Este é um bloco modded"
			// Polish: "Przykładowy blok", "Jest to modded blok"
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleBlock>());
			Item.width = 12;
			Item.height = 12;
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

		public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack) { // Calls upon use of an extractinator. Below is the chance you will get ExampleOre from the extractinator.
			if (Main.rand.NextBool(3)) {
				resultType = ModContent.ItemType<ExampleOre>();  // Get this from the extractinator with a 1 in 3 chance.
				if (Main.rand.NextBool(5)) {
					resultStack += Main.rand.Next(2); // Add a chance to get more than one of ExampleOre from the extractinator.
				}
			}
		}
	}
}
