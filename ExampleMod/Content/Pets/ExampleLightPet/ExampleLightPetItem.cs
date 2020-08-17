using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Pets.ExampleLightPet
{
	public class ExampleLightPetItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Annoying Light");
			Tooltip.SetDefault("Summons an annoying light");
		}

		public override void SetDefaults() {
			item.damage = 0;
			item.useStyle = ItemUseStyleID.Swing;
			item.shoot = ProjectileType<ExampleLightPetProjectile>();
			item.width = 16;
			item.height = 30;
			item.UseSound = SoundID.Item2;
			item.useAnimation = 20;
			item.useTime = 20;
			item.rare = ItemRarityID.Yellow;
			item.noMelee = true;
			item.value = Item.sellPrice(0, 5, 50);
			item.buffType = BuffType<ExampleLightPetBuff>();
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddRecipeGroup(RecipeGroupID.Fireflies, 10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}

		public override void UseStyle(Player player) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(item.buffType, 3600);
			}
		}
	}
}