using ExampleMod.Items.Abomination;
using Terraria;
using Terraria.ID;
// If you are using c# 6, you can use: "using static Terraria.Localization.GameCulture;" which would mean you could just write "DisplayName.AddTranslation(German, "");"
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Placeable
{
	public class ExampleBlock : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded block.");
			ItemID.Sets.ExtractinatorMode[item.type] = item.type;

			// This is an example of how translations are coded into the game. Making your mod Open Source is a good way to enlist help with translations and make your mod more popular worldwide. Be sure to have "using Terraria.Localization".
			DisplayName.AddTranslation(GameCulture.German, "Beispielblock");
			Tooltip.AddTranslation(GameCulture.German, "Dies ist ein modded Block");
			DisplayName.AddTranslation(GameCulture.Italian, "Blocco di esempio");
			Tooltip.AddTranslation(GameCulture.Italian, "Questo è un blocco moddato");
			DisplayName.AddTranslation(GameCulture.French, "Bloc d'exemple");
			Tooltip.AddTranslation(GameCulture.French, "C'est un bloc modgé");
			DisplayName.AddTranslation(GameCulture.Spanish, "Bloque de ejemplo");
			Tooltip.AddTranslation(GameCulture.Spanish, "Este es un bloque modded");
			DisplayName.AddTranslation(GameCulture.Russian, "Блок примера");
			Tooltip.AddTranslation(GameCulture.Russian, "Это модифицированный блок");
			DisplayName.AddTranslation(GameCulture.Chinese, "例子块");
			Tooltip.AddTranslation(GameCulture.Chinese, "这是一个修改块");
			DisplayName.AddTranslation(GameCulture.Portuguese, "Bloco de exemplo");
			Tooltip.AddTranslation(GameCulture.Portuguese, "Este é um bloco modded");
			DisplayName.AddTranslation(GameCulture.Polish, "Przykładowy blok");
			Tooltip.AddTranslation(GameCulture.Polish, "Jest to modded blok");
		}

		public override void SetDefaults() {
			item.width = 12;
			item.height = 12;
			item.maxStack = 999;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleBlock>();
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.SetResult(this, 10);
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleWall>(), 4);
			recipe.SetResult(this);
			recipe.AddTile(TileType<Tiles.ExampleWorkbench>());
			recipe.AddRecipe();
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExamplePlatform>(), 2);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void ExtractinatorUse(ref int resultType, ref int resultStack) {
			if (Main.rand.NextBool(30)) {
				resultType = ItemType<FoulOrb>();
				if (Main.rand.NextBool(5)) {
					resultStack += Main.rand.Next(2);
				}
			}
		}
	}
}
