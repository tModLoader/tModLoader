using ExampleMod.Content.Projectile;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
   
    public class ExampleFlamethrower : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Flamethrower;
		int speedX;
		int speedY;
		

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("This is a modded FlameThrower\nShoots a jet of cursed flames\nHas a 50% chance to not consume gel.");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 12; // The item's damage.
			Item.DamageType = DamageClass.Ranged;
			Item.width = 50;
			Item.height = 18;
			// A useTime of 4 with a useAnimation of 20 means this weapon will shoot out 5 jets of fire in one shot.
			// Vanilla Flamethrower uses values of 6 and 30 respectively, which is also 5 jets in one shot, but over 30 frames instead of 20.
			Item.useTime = 4;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true; // So the item's animation doesn't do damage
			Item.knockBack = 2; // A high knockback. Vanilla Flamethrower uses 0.3f for a weak knockback.
			Item.value = 10000;
			Item.color = new Color(61, 252, 3); // Makes the item color green, since we are reusing vanilla sprites for simplicity.
			Item.rare = ItemRarityID.Green; // Sets the item's rarity.
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<ExampleFlame>();
			Item.shootSpeed = 11f; // How fast the flames will travel. Vanilla Flamethrower uses 7f and consequentially has less reach. item.shootSpeed and projectile.timeLeft together control the range.
			Item.useAmmo = AmmoID.Gel; // Makes the weapon use up Gel as ammo
		}

		// Vanilla Flamethrower uses the commented out code below to prevent shooting while underwater, but this weapon can shoot underwater, so we don't use this code. The projectile also is specifically programmed to survive underwater.
		/*public override bool CanUseItem(Player player)
		{
			return !player.wet;
		}*/

		public override bool CanConsumeAmmo(Player player)
		{
			return Main.rand.NextFloat() >= 0.50f; 
		}




		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 54f;
			if (Collision.CanHit(position, 6, 6, position + muzzleOffset, 6, 6))
			{
				position += muzzleOffset;
			}
		}

	}
}