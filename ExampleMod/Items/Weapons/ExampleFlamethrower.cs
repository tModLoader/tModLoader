using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Projectiles;

namespace ExampleMod.Items.Weapons
{
	// Flamethrowers have some special characteristics, such as shooting several projectiles in one click, and only consuming ammo on the first projectile
	// The most important characteristics, however, are explained in the FlamethrowerProjectile code.
	public class ExampleFlamethrower : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.Flamethrower;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Shoots a jet of cursed flames\nHas a 66% chance to not consume gel\nThis is a modded flamethrower.");
		}

		public override void SetDefaults()
		{
			item.damage = 69; // The item's damage.
			item.ranged = true;
			item.width = 50;
			item.height = 18;
			// A useTime of 4 with a useAnimation of 20 means this weapon will shoot out 5 jets of fire in one shot.
			// Vanilla Flamethrower uses values of 6 and 30 respectively, which is also 5 jets in one shot, but over 30 frames instead of 20.
			item.useTime = 4;
			item.useAnimation = 20; 
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true; // So the item's animation doesn't do damage
			item.knockBack = 2; // A high knockback. Vanilla Flamethrower uses 0.3f for a weak knockback.
			item.value = 10000;
			item.color = new Color(61, 252, 3); // Makes the item color green, since we are reusing vanilla sprites for simplicity.
			item.rare = ItemRarityID.Green; // Sets the item's rarity.
			item.UseSound = SoundID.Item34;
			item.autoReuse = true;
			item.shoot = ModContent.ProjectileType<FlamethrowerProjectile>();
			item.shootSpeed = 9f; // How fast the flames will travel. Vanilla Flamethrower uses 7f and consequentially has less reach. item.shootSpeed and projectile.timeLeft together control the range.
			item.useAmmo = AmmoID.Gel; // Makes the weapon use up Gel as ammo
		}

		// Vanilla Flamethrower uses the commented out code below to prevent shooting while underwater, but this weapon can shoot underwater, so we don't use this code. The projectile also is specifically programmed to survive underwater.
		/*public override bool CanUseItem(Player player)
		{
			return !player.wet;
		}*/

		public override bool ConsumeAmmo(Player player)
		{
			// To make this item only consume ammo during the first jet, we check to make sure the animation just started. ConsumeAmmo is called 5 times because of item.useTime and item.useAnimation values in SetDefaults above.
			return player.itemAnimation >= player.itemAnimationMax - 4;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Flamethrower);
			recipe.AddIngredient(ModContent.ItemType<ExampleItem>(), 3);
			recipe.AddIngredient(ItemID.CursedFlame, 15);
			recipe.AddTile(ModContent.TileType<Tiles.ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
			Vector2 muzzleOffset = Vector2.Normalize(new Vector2(speedX, speedY)) * 54f; //This gets the direction of the flame projectile, makes its length to 1 by normalizing it. It then multiplies it by 54 (the item width) to get the position of the tip of the flamethrower.
			if (Collision.CanHit(position, 6, 6, position + muzzleOffset, 6, 6))
			{
				position += muzzleOffset;
			}
			// This is to prevent shooting through blocks and to make the fire shoot from the muzzle.
			return true;
        }
        public override Vector2? HoldoutOffset()
		// HoldoutOffset has to return a Vector2 because it needs two values (an X and Y value) to move your flamethrower sprite. Think of it as moving a point on a cartesian plane.
		{
			return new Vector2(0, -2); // If your own flamethrower is being held wrong, edit these values. You can test out holdout offsets using Modder's Toolkit.
		}
	}
}
