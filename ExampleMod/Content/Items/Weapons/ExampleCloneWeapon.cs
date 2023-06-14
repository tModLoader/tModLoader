using ExampleMod.Content.Projectiles;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	/// <summary>
	/// This weapon and its corresponding projectile showcase the CloneDefaults() method, which allows for cloning of other items.
	/// For this example, we shall copy the Meowmere and its projectiles with the CloneDefaults() method, while also changing them slightly.
	/// For a more detailed description of each item field used here, check out <see cref="ExampleSword" />.
	/// </summary>
	public class ExampleCloneWeapon : ModItem
	{
		public override void SetDefaults() {
			// This method right here is the backbone of what we're doing here; by using this method, we copy all of
			// the meowmere's SetDefault stats (such as Item.melee and Item.shoot) on to our item, so we don't have to
			// go into the source and copy the stats ourselves. It saves a lot of time and looks much cleaner; if you're
			// going to copy the stats of an item, use CloneDefaults().

			Item.CloneDefaults(ItemID.Meowmere);

			// After CloneDefaults has been called, we can now modify the stats to our wishes, or keep them as they are.
			// For the sake of example, let's swap the vanilla Meowmere projectile shot from our item for our own projectile by changing Item.shoot:

			Item.shoot = ModContent.ProjectileType<ExampleCloneProjectile>(); // Remember that we must use ProjectileType<>() since it is a modded projectile!
			// Check out ExampleCloneProjectile to see how this projectile is different from the Vanilla Meowmere projectile.

			// While we're at it, let's make our weapon's stats a bit stronger than the Meowmere, which can be done
			// by using math on each given stat.

			Item.damage *= 2; // Makes this weapon's damage double the Meowmere's damage.
			Item.shootSpeed *= 1.25f; // Makes this weapon's projectiles shoot 25% faster than the Meowmere's projectiles.
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
