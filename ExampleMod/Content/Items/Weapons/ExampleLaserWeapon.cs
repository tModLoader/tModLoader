using ExampleMod.Projectiles;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleLaserWeapon : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Laser Weapon")
			Tooltip.SetDefault("Shoot a laser beam that can eliminate anything...");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 30;
			Item.damage = 40;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Magic;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.mana = 5;
			Item.rare = ItemRarityID.Pink;
			Item.useTime = 20;
			Item.UseSound = SoundID.Item13;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shootSpeed = 14f;
			Item.useAnimation = 20;
			Item.shoot = ProjectileType<ExampleLaser>();
			Item.value = Item.sellPrice(silver: 3);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(1)
				.AddTile(ModContent.TileType<Tiles.Furniture.ExampleWorkbench>())
				.Register();
		}
	}
}
