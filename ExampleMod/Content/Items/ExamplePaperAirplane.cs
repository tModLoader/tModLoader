using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExamplePaperAirplane : ModItem
	{
		public override void SetDefaults() {
			Item.width = 22; // The item texture's width
			Item.height = 16; // The item texture's height

			Item.value = Item.sellPrice(0, 0, 10); // The value of the item. In this case, 10 silver. Item.buyPrice & Item.sellPrice are helper methods that returns costs in copper coins based on platinum/gold/silver/copper arguments provided to it.

			Item.DefaultToThrownWeapon(ModContent.ProjectileType<Projectiles.ExamplePaperAirplaneProjectile>(), 17, 5f); // A special method that sets a variety of item parameters that make the item act like a throwing weapon.

			// The above Item.DefaultToThrownWeapon() does the following. Uncomment these if you don't want to use the above method or want to change something about it.
			// Item.autoReuse = false;
			// Item.useStyle = ItemUseStyleID.Swing;
			// Item.useAnimation = 17;
			// Item.useTime = 17;
			// Item.shoot = ModContent.ProjectileType<Projectiles.ExamplePaperAirplaneProjectile>();
			// Item.shootSpeed = 5f;
			// Item.noMelee = true;
			// Item.DamageType = DamageClass.Ranged;
			// Item.consumable = true;
			// Item.maxStack = Item.CommonMaxStack;

			Item.SetWeaponValues(4, 2f); // A special method that sets the damage, knockback, and bonus critical strike chance.

			// The above Item.SetWeaponValues() does the following. Uncomment these if you don't want to use the above method.
			// Item.damage = 4;
			// Item.knockBack = 2;
			// Item.crit = 0; // Even though this says 0, this is more like "bonus critical strike chance". All weapons have a base critical strike chance of 4.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe(10)
				.AddIngredient(ModContent.ItemType<ExampleItem>())
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
