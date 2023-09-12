using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ExampleMod.Content.Tiles.Furniture;
using ExampleMod.Content.Projectiles;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleLaserWeapon : ModItem
	{
		public override void SetDefaults() {
			Item.damage = 40;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Magic;
			Item.channel = true; //Channel so that you can held the weapon [Important].
			Item.mana = 5;
			Item.rare = ItemRarityID.Pink;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.UseSound = SoundID.Item13;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shootSpeed = 14f;
			Item.useAnimation = 20;
			Item.shoot = ModContent.ProjectileType<ExampleLaser>();
			Item.value = Item.sellPrice(silver: 3);
		}

		public override void AddRecipes() {
			CreateRecipe(1).
			AddIngredient(ModContent.ItemType<ExampleItem>(), 10).
			AddTile(ModContent.TileType<ExampleWorkbench>()).
			Register();
		}
	}
}
