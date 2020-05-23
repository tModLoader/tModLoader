using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Weapons
{
	internal class ExampleExplosive : ModItem
	{
		// TODO, count as explosive for demolitionist spawn

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Example Explosive");
		}

		public override void SetDefaults() {
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.shootSpeed = 12f;
			item.shoot = ProjectileType<Projectiles.ExampleExplosive>();
			item.width = 8;
			item.height = 28;
			item.maxStack = 30;
			item.consumable = true;
			item.UseSound = SoundID.Item1;
			item.useAnimation = 40;
			item.useTime = 40;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.value = Item.buyPrice(0, 0, 20, 0);
			item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>());
			recipe.AddIngredient(ItemID.Dynamite);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
