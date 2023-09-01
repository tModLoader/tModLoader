using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Content.Projectiles;
using ExampleMod.Content.Tiles.Furniture;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Type] = true; // This makes the useStyle animate as a staff instead of as a gun.
		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // This is set to true so the animation doesn't deal damage.
			Item.knockBack = 5;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item20;
			Item.autoReuse = true;
			Item.shoot = ProjectileType<SparklingBall>();
			Item.shootSpeed = 16f;
		}

		public override void AddRecipes()
		{
            CreateRecipe().
                AddIngredient<ExampleItem>().
                AddTile<ExampleWorkbench>().
                Register();
        }
	}
}