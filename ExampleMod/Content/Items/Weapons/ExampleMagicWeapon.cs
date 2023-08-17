using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleMagicWeapon : ModItem
	{
		public override void SetDefaults() {
			// DefaultToStaff handles setting various Item values that magic staff weapons use.
			// Hover over DefaultToStaff in Visual Studio to read the documentation!
			// Shoot a black bolt, also known as the projectile shot from the onyx blaster.
			Item.DefaultToStaff(ProjectileID.BlackBolt, 7, 20, 11);
			Item.width = 34;
			Item.height = 40;
			Item.UseSound = SoundID.Item71;

			// A special method that sets the damage, knockback, and bonus critical strike chance.
			// This weapon has a crit of 32% which is added to the players default crit chance of 4%
			Item.SetWeaponValues(25, 6, 32);

			Item.SetShopValues(ItemRarityColor.LightRed4, 10000);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
