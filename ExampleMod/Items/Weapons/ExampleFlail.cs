using ExampleMod.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class ExampleFlail : ModItem
	{
		public override void SetDefaults()
		{
			item.width = 22;
			item.height = 20;
			item.value = Item.sellPrice(silver: 5);
			item.rare = ItemRarityID.White;
			item.noMelee = true;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useAnimation = 40;
			item.useTime = 40;
			item.knockBack = 4f;
			item.damage = 9;
			item.noUseGraphic = true;
			item.shoot = ModContent.ProjectileType<ExampleFlailProjectile>();
			item.shootSpeed = 15.1f;
			item.UseSound = SoundID.Item1;
			item.melee = true;
			item.crit = 9;
			item.channel = true;
		}

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Chain, 1);
			recipe.AddIngredient(ModContent.ItemType<ExampleItem>(), 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}